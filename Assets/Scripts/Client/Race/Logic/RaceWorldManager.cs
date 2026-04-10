using SyncSample.Client.Gameplay;
using SyncSample.Common;
using SyncSample.Common.Model.Race;
using UnityEngine;

namespace SyncSample.Client.Race.Logic
{
    /// <summary>
    /// Race 状态同步世界管理器：
    /// 1. 维护本地/服务器帧号与逻辑帧时长；
    /// 2. 发送本地输入；
    /// 3. 在开启客户端预测时驱动车辆本地预测。
    /// </summary>
    public class RaceWorldManager : IUpdatable
    {
        private static RaceWorldManager _instance;
        public static RaceWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RaceWorldManager();
                return _instance;
            }
        }

        private long _localFrame;
        private long _serverFrame;
        private float _frameDeltaTime;
        private float _accumulatedTime;
        /// <summary> 是否已收到过至少一条 WorldState（可开始按服务器节奏发输入等）。 </summary>
        private bool _hasReceivedFirstWorldState;

        public long LocalFrame => _localFrame;
        public long ServerFrame => _serverFrame;
        public float FrameDeltaTime => _frameDeltaTime;

        public void Initialize()
        {
            ResetSession();
        }

        public void ResetSession()
        {
            _localFrame = 0;
            _serverFrame = 0;
            _hasReceivedFirstWorldState = false;
            _accumulatedTime = 0f;
        }

        /// <summary>
        /// 当前 Race 协议可以直接传入车辆数组时使用。
        /// </summary>
        public void UpdateWorldState(RaceWorldStateMessage state)
        {
            if (state == null)
                return;

            if (!_hasReceivedFirstWorldState)
            {
                _hasReceivedFirstWorldState = true;
                _accumulatedTime = 0;
                _frameDeltaTime = state.frameDeltaTime;
                _localFrame = _serverFrame = state.frame;
                Logger.Log($"[SyncStateWorld] 首次 WorldState，初始化并同步服务器帧号 frame={state.frame}, frameDeltaTime={state.frameDeltaTime}");
            }
            else
            {
                _serverFrame = state.frame;
                if (_localFrame + 1 < _serverFrame)
                {
                    Logger.Log($"[SyncStateWorld] 前后端帧号差距过大，直接同步服务器帧号，服务器帧号：{_serverFrame}，本地帧号：{_localFrame}");
                    _accumulatedTime = 0;
                    _localFrame = _serverFrame;
                }
            }

            VehicleManager.Instance.ApplyServerWorldState(state.frame, state.vehicles);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasReceivedFirstWorldState)
                return;
            
            _accumulatedTime += deltaTime;
            while (_accumulatedTime >= _frameDeltaTime)
            {
                _accumulatedTime -= _frameDeltaTime;
                _localFrame++;

                ProcessInput(_localFrame + 1);
            }
        }

        private void ProcessInput(long inputFrame)
        {
            int inputValue = InputManager.Instance.GetInput();
            var msg = new PlayerInputMessage(inputFrame, inputValue);
            string json = JsonUtility.ToJson(msg);
            int sendDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddSendDelay : 0;
            if (sendDelayMs > 0)
            {
                var client = GameMain.Instance.Client;
                GameMain.Instance.GameLooper.RunAfterDelayMilliseconds(sendDelayMs, () =>
                {
                    if (client != null && client.IsConnected)
                        client.Send(MessageType.PlayerInput, json);
                });
            }
            else
            {
                GameMain.Instance.Client.Send(MessageType.PlayerInput, json);
            }

            if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                VehicleManager.Instance.ReceiveInput(
                    inputFrame,
                    inputValue.GetHorizontal(),
                    inputValue.GetVertical(),
                    _frameDeltaTime);
            }
        }
    }
}
