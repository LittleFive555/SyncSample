using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Server.Gameplay
{
    public class LockstepSyncManager
    {
        private static LockstepSyncManager _instance;
        public static LockstepSyncManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LockstepSyncManager();
                return _instance;
            }
        }

        private TcpGameServer _server;
        private Thread _updateThread;

        private long _currentFrame;
        private long _accumulatedLogicTimeMs;
        private Dictionary<string, PlayerInputMessage> _playerInputs = new Dictionary<string, PlayerInputMessage>();

        public void Start(TcpGameServer server)
        {
            _server = server;
            _currentFrame = 0;
            _updateThread = new Thread(UpdateLoop) { IsBackground = true };
            _updateThread.Start();
        }

        public void AppendPlayerInput(string clientId, PlayerInputMessage input)
        {
            if (input.frame != _currentFrame) // NOTE 简单处理，非本帧数据直接丢弃
            {
                Logger.LogWarning("Skip input: clientId=" + clientId + " frame " + input.frame + " " + _currentFrame);
                return;
            }

            lock (_playerInputs)
            {
                _playerInputs[clientId] = input;
            }
        }

        private void Update(long frame, float deltaTime)
        {
            var allPlayerInputMessage = new AllPlayerInputMessage(frame, _playerInputs.Values.ToList());
            _server.Broadcast(MessageType.AllPlayerInput, JsonUtility.ToJson(allPlayerInputMessage));
            _playerInputs.Clear();
        }

        /// <summary>
        /// 使用 Stopwatch 频率对齐系统高精度时钟，按固定 _frameTime 推进逻辑；
        /// 落后时在同一轮内连续补帧直到追上时钟，过快则睡到下一帧边界。
        /// </summary>
        private void UpdateLoop()
        {
            long frameTicks = (long)(GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime * Stopwatch.Frequency);
            long nextFrameTick = Stopwatch.GetTimestamp();

            while (true)
            {
                long now = Stopwatch.GetTimestamp();

                while (now >= nextFrameTick)
                {
                    Update(_currentFrame, GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime);
                    _currentFrame++;
                    _accumulatedLogicTimeMs += (long)(GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime * 1000);
                    nextFrameTick += frameTicks;
                    now = Stopwatch.GetTimestamp();
                }

                now = Stopwatch.GetTimestamp();
                long remainingTicks = nextFrameTick - now;
                if (remainingTicks > 0)
                {
                    double sleepMs = remainingTicks * 1000.0 / Stopwatch.Frequency;
                    if (sleepMs >= 1.0)
                        Thread.Sleep((int)sleepMs);
                    else
                        Thread.Sleep(0); // 让出时间片，剩余由下一轮对齐
                }
                else
                    Thread.Sleep(0);
            }
        }
    }
}