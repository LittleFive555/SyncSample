using System.Collections.Generic;
using SyncSample.Common;

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

        public float LogicFixedDeltaTime { get; set; } = 0.03333333f;

        public void Initialize()
        {
            Logger.Log("WorldManager Initialize");
        }

        public void OnUpdate(float deltaTime)
        {
            if (!LockstepPlayerInputSync.AllClientsConnected()) return;

            if (!_isWaitingForAllClients) // 如果在等待所有客户端输入，则不累积时间
                _accumulatedTime += deltaTime;

            // 接收延迟在 LockstepMessageHandlers 中延迟入队，不拉长本地逻辑帧间隔（否则会错误改变帧率）
            while (_accumulatedTime >= LogicFixedDeltaTime)
            {
                if (!LockstepPlayerInputSync.HasAllInputsForFrame(_currentFrame))
                {
                    _isWaitingForAllClients = true;
                    return;
                }
                _isWaitingForAllClients = false;
                _accumulatedTime -= LogicFixedDeltaTime;
                LockstepPlayerInputSync.ApplyFrame(_currentFrame);
                // 2. 推进帧
                AdvanceFrame();
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
    }
}
