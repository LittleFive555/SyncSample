namespace SyncSample.Client.Gameplay.World.Logic
{
    public interface ILogicUpdate
    {
        int Priority { get; }
        void OnLogicFrame(long frame);
    }
}