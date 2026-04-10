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
        // RTT 的平滑值，用来估算单程网络延迟。
        private float _smoothedRttMs;
        private bool _hasRttSample;
        // 本地时钟与“服务器时间 + 单程延迟”之间的误差，后续在 Update 中逐步吃掉。
        private float _clockErrorSeconds;
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
            _smoothedRttMs = 0f;
            _hasRttSample = false;
            _clockErrorSeconds = 0f;
        }

        public void UpdateRtt(long rttMs)
        {
            float sampleMs = Mathf.Max(0f, rttMs);
            if (!_hasRttSample)
            {
                _smoothedRttMs = sampleMs;
                _hasRttSample = true;
                return;
            }

            _smoothedRttMs = Mathf.Lerp(_smoothedRttMs, sampleMs, 0.2f);
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
                _frameDeltaTime = state.frameDeltaTime;
                _serverFrame = state.frame;
                // 首包直接把本地逻辑时钟放到目标位置，避免一开始慢慢追钟。
                SetLocalClock(GetTargetLocalTimeSeconds(state.frame));
                Logger.Log($"[SyncStateWorld] 首次 WorldState，初始化并同步服务器帧号 frame={state.frame}, frameDeltaTime={state.frameDeltaTime}, rtt={_smoothedRttMs:F1}ms");
            }
            else
            {
                _serverFrame = state.frame;
                _frameDeltaTime = state.frameDeltaTime;

                float targetLocalTime = GetTargetLocalTimeSeconds(state.frame);
                float localTime = GetLocalTimeSeconds();
                _clockErrorSeconds = targetLocalTime - localTime;

                // 偏差特别大时直接拉齐，避免本地帧号和服务器差太远。
                if (Mathf.Abs(_clockErrorSeconds) > _frameDeltaTime * GlobalSwitch.Instance.StateSyncSwitch.SnapThresholdInFrames)
                {
                    Logger.Log($"[SyncStateWorld] RTT 校时差距过大，直接校正。serverFrame={_serverFrame}, localFrame={_localFrame}, error={_clockErrorSeconds:F3}s, rtt={_smoothedRttMs:F1}ms");
                    SetLocalClock(targetLocalTime);
                    _clockErrorSeconds = 0f;
                }
            }

            VehicleManager.Instance.ApplyServerWorldState(state.frame, state.vehicles);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_hasReceivedFirstWorldState)
                return;

            // 每帧只吃掉一小部分时钟误差，避免视觉和输入帧号出现突跳。
            float correctedDeltaTime = deltaTime + ConsumeClockCorrection(deltaTime);
            _accumulatedTime += correctedDeltaTime;
            while (_accumulatedTime >= _frameDeltaTime)
            {
                _accumulatedTime -= _frameDeltaTime;
                _localFrame++;

                ProcessInput(_localFrame + 1);
            }
        }

        private float GetTargetLocalTimeSeconds(long serverFrame)
        {
            // 简化算法：
            // 收到 frame=N 的快照时，服务器实际上已经又往前跑了大约半个 RTT，
            // 所以客户端目标时钟取“服务器帧时间 + RTT/2”。
            return serverFrame * _frameDeltaTime + GetEstimatedOneWayDelaySeconds();
        }

        private float GetEstimatedOneWayDelaySeconds()
        {
            return _hasRttSample ? _smoothedRttMs * 0.001f * 0.5f : 0f;
        }

        private float GetLocalTimeSeconds()
        {
            return _localFrame * _frameDeltaTime + _accumulatedTime;
        }

        private void SetLocalClock(float targetTimeSeconds)
        {
            if (_frameDeltaTime <= 0.0001f)
            {
                _localFrame = 0;
                _accumulatedTime = 0f;
                return;
            }

            targetTimeSeconds = Mathf.Max(0f, targetTimeSeconds);
            _localFrame = Mathf.FloorToInt(targetTimeSeconds / _frameDeltaTime);
            _accumulatedTime = targetTimeSeconds - _localFrame * _frameDeltaTime;
        }

        private float ConsumeClockCorrection(float deltaTime)
        {
            if (Mathf.Abs(_clockErrorSeconds) <= 0.0001f)
                return 0f;

            // 限制每帧最多修正一小段时间，防止本地时钟一下子快/慢太多。
            float maxCorrection = deltaTime * GlobalSwitch.Instance.StateSyncSwitch.ClockCorrectionRate;
            float correction = Mathf.Clamp(_clockErrorSeconds, -maxCorrection, maxCorrection);
            _clockErrorSeconds -= correction;
            return correction;
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
