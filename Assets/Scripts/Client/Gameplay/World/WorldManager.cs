namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 基于 FixedUpdate 的帧更新器。内部使用独立的 UpdateDispatcher 驱动本帧逻辑下的模块（如 PlayerInputSender），与 GameMain 的 Dispatcher 分离。
    /// </summary>
    public class WorldManager : IFixedUpdatable
    {
        private static WorldManager _instance;
        public static WorldManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WorldManager();
                return _instance;
            }
        }

        private long _currentFrame;

        /// <summary> 当前世界帧号，每执行一次 FixedUpdate 递增 1。 </summary>
        public long CurrentFrame => _currentFrame;

        public void Initialize()
        {
            Logger.Log("WorldManager Initialize");
        }

        /// <summary> 固定时间步长（秒），与 Physics.fixedDeltaTime 一致，用于逻辑帧计算。 </summary>
        public float FixedDeltaTime { get; private set; }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            FixedDeltaTime = fixedDeltaTime;
            // 1. 等待并应用所有客户端输入
            if (!PlayerInputSync.HasAllInputsForFrame(_currentFrame + 1))
                return;
            WaitForAllClientsThisFrame();
            // 2. 推进帧
            AdvanceFrame();
        }

        /// <summary>
        /// 本帧已收齐所有客户端输入（lockstep）：应用玩家输入后推进帧。
        /// </summary>
        protected virtual void WaitForAllClientsThisFrame()
        {
            PlayerInputSync.ApplyFrame(_currentFrame);
        }

        /// <summary> 推进一帧。子类可重写以在推进前/后插入逻辑。 </summary>
        protected virtual void AdvanceFrame()
        {
            _currentFrame++;
        }
    }
}
