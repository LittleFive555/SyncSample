using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay.StateSync.World.Logic
{
    /// <summary>
    /// 状态同步世界：仅在收到服务器的 WorldState 时更新世界与帧号，不在 Update 中做本地推演。
    /// </summary>
    public class SyncStateWorldManager : IUpdatable
    {
        private static SyncStateWorldManager _instance;
        public static SyncStateWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SyncStateWorldManager();
                return _instance;
            }
        }

        private long _localFrame;
        /// <summary> 与服务器最近一次快照对应的逻辑帧号。 </summary>
        private long _serverFrame;
        /// <summary> 服务器逻辑帧时长（最近一次 WorldState）。 </summary>
        private float _frameDeltaTime;
        public float FrameDeltaTime => _frameDeltaTime;
        /// <summary> 是否已收到过至少一条 WorldState（可开始按服务器节奏发输入等）。 </summary>
        private bool _hasReceivedFirstWorldState;

        private float _accumulatedTime;

        public void Initialize()
        {
            _hasReceivedFirstWorldState = false;
            _serverFrame = 0;
            _frameDeltaTime = 0f;
        }

        /// <summary> 断线重连等场景下重置，使下一条 WorldState 再次走首次初始化。 </summary>
        public void ResetSession()
        {
            _hasReceivedFirstWorldState = false;
            _serverFrame = 0;
            _frameDeltaTime = 0f;
        }

        /// <summary> 由网络层在收到 WorldState 时调用：首次包做初始化并同步帧参数，此后每次应用快照并更新帧号。 </summary>
        public void UpdateWorldState(WorldStateMessage state)
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
                // TODO 前后端帧号差距过大时

            }

            CharacterManager.Instance.ApplyServerWorldState(state.characters);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasReceivedFirstWorldState)
                return;
            
            _accumulatedTime += deltaTime;

            // 接收延迟在 LockstepMessageHandlers 中延迟入队，不拉长本地逻辑帧间隔（否则会错误改变帧率）
            while (_accumulatedTime >= _frameDeltaTime)
            {
                _accumulatedTime -= _frameDeltaTime;
                _localFrame++;
            }

            ProcessInput();
        }

        private long _lastSentFrame = -1;
        private void ProcessInput()
        {
            int inputValue = InputManager.Instance.GetInput();
            if (inputValue != 0)
            {
                // 简化处理，一帧只能发送一个操作
                // 如果帧时间较长，或者操作较快，可以考虑每帧打包发送多个操作、或者每次有操作时立即发送
                long currentFrame = _localFrame + 1;
                if (currentFrame == _lastSentFrame)
                    return;

                _lastSentFrame = currentFrame;

                var msg = new PlayerInputMessage(currentFrame, inputValue);
                string json = JsonUtility.ToJson(msg);
                int sendDelayMs = GlobalSwitch.Instance != null ? GlobalSwitch.Instance.AddSendDelay : 0;
                if (sendDelayMs > 0) // 延迟模拟
                {
                    var c = GameMain.Instance.Client;
                    GameMain.Instance.GameLooper.RunAfterDelayMilliseconds(sendDelayMs, () =>
                    {
                        if (c != null && c.IsConnected)
                            c.Send(MessageType.PlayerInput, json);
                    });
                }
                else
                    GameMain.Instance.Client.Send(MessageType.PlayerInput, json);
                
                
                if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
                {
                    CharacterManager.Instance.ReceiveInput(currentFrame, inputValue.GetHorizontal(), inputValue.GetVertical());
                }
            }
        }
    }
}