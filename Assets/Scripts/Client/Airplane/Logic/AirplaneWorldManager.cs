using System.Collections.Generic;
using SyncSample.Client.Gameplay;
using SyncSample.Client.Gameplay.Lockstep;
using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Airplane.Logic
{
    /// <summary>
    /// 基于 FixedUpdate 的帧更新器。内部使用独立的 UpdateDispatcher 驱动本帧逻辑下的模块（如 PlayerInputSender），与 GameMain 的 Dispatcher 分离。
    /// </summary>
    public class AirplaneWorldManager : IUpdatable
    {
        private static AirplaneWorldManager _instance;
        public static AirplaneWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AirplaneWorldManager();
                return _instance;
            }
        }

        private SortedList<int, List<ILogicUpdate>> _logicEntities = new SortedList<int, List<ILogicUpdate>>();

        private readonly List<long> _enemySpawnFrames = new List<long>()
        {
            10,
            20,
            30,
            40,
            50,
            60,
            70,
            80,
            90,
            100
        };

        private long _currentFrame;

        /// <summary> 当前世界帧号，每执行一次 FixedUpdate 递增 1。 </summary>
        public long CurrentFrame => _currentFrame;

        private float _accumulatedTime;
        private bool _isWaitingForAllClients;
        public bool IsBlockedForSyncing => _isWaitingForAllClients;

        public float LogicFixedDeltaTime => GlobalSwitch.Instance.LockstepSwitch.LogicDeltaTime;

        public void Initialize()
        {
            Logger.Log("WorldManager Initialize");
        }

        public void OnUpdate(float deltaTime)
        {
            if (!AirplanePlayerInputSync.AllClientsConnected()) return;

            // 可能需要规定一个时间，比如在一帧内的百分之多少来发操作
            ProcessInput();
            
            if (GlobalSwitch.Instance.LockstepSwitch.Optimistic) // 乐观锁步
            {
                if (!AirplanePlayerInputSync.TryGetFrameMessage(_currentFrame + 1, out AllPlayerInputMessage message))
                    return;
                AirplanePlayerInputSync.ApplyFrameMessage(message);
                AdvanceFrame();
            }
        }

        public void RegisterLogicEntity(ILogicUpdate entity)
        {
            if (_logicEntities.TryGetValue(entity.Priority, out var list))
            {
                if (!list.Contains(entity))
                {
                    list.Add(entity);
                }
            }
            else
            {
                list = new List<ILogicUpdate>() { entity };
                _logicEntities.Add(entity.Priority, list);
            }
        }

        public void UnregisterLogicEntity(ILogicUpdate entity)
        {
            if (_logicEntities.TryGetValue(entity.Priority, out var list) && list.Contains(entity))
            {
                list.Remove(entity);
            }
        }

        /// <summary> 推进一帧。子类可重写以在推进前/后插入逻辑。 </summary>
        protected virtual void AdvanceFrame()
        {
            foreach (var entity in _logicEntities)
            {
                foreach (var e in entity.Value)
                {
                    e.OnLogicFrame(_currentFrame);
                }
            }
            if (_enemySpawnFrames.Contains(_currentFrame))
            {
                AirplaneManager.Instance.NewEnemy();
            }
            _currentFrame++;
        }

        private long _lastSentFrame = -1;
        private void ProcessInput()
        {
            // 简化处理，一帧只能发送一个操作
            // 如果帧时间较长，或者操作较快，可以考虑每帧打包发送多个操作、或者每次有操作时立即发送
            long currentFrame = CurrentFrame + 1;
            if (currentFrame <= _lastSentFrame)
                return;

            _lastSentFrame = currentFrame;
            var input = InputManager.Instance.GetInput();
            var msg = new PlayerInputMessage(currentFrame, input);
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
        }
    }
}