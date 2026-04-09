namespace SyncSample.Client.Gameplay.World.Logic
{
    public interface ICharacterEntity
    {
        string Id { get; }
        string Name { get; }
        bool IsLocal { get; }
        float X { get; }
        float Y { get; }
    }
}