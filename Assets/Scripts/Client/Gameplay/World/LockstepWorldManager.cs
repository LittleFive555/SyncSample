using System.Collections.Generic;
using SyncSample.Common;
using UnityEngine;

namespace SyncSample.Client.Gameplay
{
    public interface ILogicEntity
    {
        int Priority { get; }
        void OnLogicFrame(long frame);
    }
    
    /// <summary>
    /// 基于 FixedUpdate 的帧更新器。内部使用独立的 UpdateDispatcher 驱动本帧逻辑下的模块（如 PlayerInputSender），与 GameMain 的 Dispatcher 分离。
    /// </summary>
    public class LockstepWorldManager : IUpdatable
    {
        private static LockstepWorldManager _instance;
        public static LockstepWorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LockstepWorldManager();
                return _instance;
            }
        }

        private SortedList<int, List<ILogicEntity>> _logicEntities = new SortedList<int, List<ILogicEntity>>();

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
            if (!LockstepPlayerInputSync.AllClientsConnected()) return;

            // 可能需要规定一个时间，比如在一帧内的百分之多少来发操作
            SendInput();
            
            if (GlobalSwitch.Instance.LockstepSwitch.Optimistic) // 乐观锁步
            {
                if (!LockstepPlayerInputSync.TryGetFrameMessage(_currentFrame + 1, out AllPlayerInputMessage message))
                    return;
                LockstepPlayerInputSync.ApplyFrameMessage(message);
                AdvanceFrame();
            }
            else // 原始锁步
            {
                if (!_isWaitingForAllClients) // 如果在等待所有客户端输入，则不累积时间
                    _accumulatedTime += deltaTime;

                // 接收延迟在 LockstepMessageHandlers 中延迟入队，不拉长本地逻辑帧间隔（否则会错误改变帧率）
                while (_accumulatedTime >= LogicFixedDeltaTime)
                {
                    if (!LockstepPlayerInputSync.HasAllInputsForFrame(_currentFrame + 1))
                    {
                        _isWaitingForAllClients = true;
                        return;
                    }
                    _isWaitingForAllClients = false;
                    _accumulatedTime -= LogicFixedDeltaTime;
                    LockstepPlayerInputSync.ApplyFrame(_currentFrame + 1);
                    // 2. 推进帧
                    AdvanceFrame();
                }
            }
        }

        public void RegisterLogicEntity(ILogicEntity entity)
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
                list = new List<ILogicEntity>() { entity };
                _logicEntities.Add(entity.Priority, list);
            }
        }

        public void UnregisterLogicEntity(ILogicEntity entity)
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
            _currentFrame++;
        }

        private long _lastSentFrame = -1;
        private void SendInput()
        {
            // 简化处理，一帧只能发送一个操作
            // 如果帧时间较长，或者操作较快，可以考虑每帧打包发送多个操作、或者每次有操作时立即发送
            long currentFrame = CurrentFrame + 1;
            if (currentFrame <= _lastSentFrame)
                return;

            _lastSentFrame = currentFrame;
            LockstepInputManager.Instance.GetInput(out float dx, out float dy);
            var msg = new PlayerInputMessage(currentFrame, FixedPoint.FromFloat(dx), FixedPoint.FromFloat(dy));
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
