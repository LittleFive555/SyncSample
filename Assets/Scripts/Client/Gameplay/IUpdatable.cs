namespace SyncSample.Client.Gameplay
{
    /// <summary>
    /// 每帧更新。实现并注册到 UpdateDispatcher 后会被安全调用（单模块异常不影响其他模块）。
    /// </summary>
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    /// <summary>
    /// 固定步长更新（如物理）。注册后参与 FixedUpdate 阶段。
    /// </summary>
    public interface IFixedUpdatable
    {
        void OnFixedUpdate(float fixedDeltaTime);
    }

    /// <summary>
    /// 延迟更新（如相机、后处理）。在当帧所有 Update 之后调用。
    /// </summary>
    public interface ILateUpdatable
    {
        void OnLateUpdate(float deltaTime);
    }
}
