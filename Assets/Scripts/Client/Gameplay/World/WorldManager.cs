namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 基于 FixedUpdate 的帧更新器。内部使用独立的 UpdateDispatcher 驱动本帧逻辑下的模块（如 PlayerInputSender），与 GameMain 的 Dispatcher 分离。
    /// </summary>
    public class WorldManager : IUpdatable
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

        private float _accumulatedTime;
        private bool _isWaitingForAllClients;

        public float LogicFixedDeltaTime { get; set; } = 0.03333333f;

        public void Initialize()
        {
            Logger.Log("WorldManager Initialize");
        }

        public void OnUpdate(float deltaTime)
        {
            if (!PlayerInputSync.AllClientsConnected()) return;

            if (!_isWaitingForAllClients) // 如果在等待所有客户端输入，则不累积时间
                _accumulatedTime += deltaTime;

            while (_accumulatedTime >= LogicFixedDeltaTime) // 如果累积时间大于逻辑固定时间步，则推进一帧
            {
                if (!PlayerInputSync.HasAllInputsForFrame(_currentFrame + 1))
                {
                    _isWaitingForAllClients = true;
                    return;
                }
                _isWaitingForAllClients = false;
                _accumulatedTime -= LogicFixedDeltaTime;
                PlayerInputSync.ApplyFrame(_currentFrame);
                // 2. 推进帧
                AdvanceFrame();
            }
        }

        /// <summary> 推进一帧。子类可重写以在推进前/后插入逻辑。 </summary>
        protected virtual void AdvanceFrame()
        {
            _currentFrame++;
        }
    }
}
