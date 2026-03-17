namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 基于 FixedUpdate 的帧更新器。每固定时间步推进一帧，将来联网时可在此处等待所有客户端就绪后再推进。
    /// </summary>
    public class WorldManager : IFixedUpdatable
    {
        private long _currentFrame;

        /// <summary> 当前世界帧号，每执行一次 FixedUpdate 递增 1。 </summary>
        public long CurrentFrame => _currentFrame;

        /// <summary> 固定时间步长（秒），与 Physics.fixedDeltaTime 一致，用于逻辑帧计算。 </summary>
        public float FixedDeltaTime { get; private set; }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            FixedDeltaTime = fixedDeltaTime;
            if (!PlayerInputSync.HasAllInputsForFrame(_currentFrame + 1))
                return;
            WaitForAllClientsThisFrame();
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
