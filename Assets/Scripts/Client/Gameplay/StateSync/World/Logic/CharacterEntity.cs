using SyncSample.Client.Gameplay.World.Logic;

namespace SyncSample.Client.Gameplay.StateSync.World.Logic
{
    public class CharacterEntity: ICharacterEntity
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsLocal { get; private set; }

        public float DeltaX { get; private set; }
        public float DeltaY { get;private set; }

        public float X { get; private set; }
        public float Y { get; private set; }

        public const float MoveSpeed = 3f;
        
        public CharacterEntity(string id, string name, bool isLocal)
        {
            Id = id;
            Name = name;
            IsLocal = isLocal;
        }

        public void ReceiveInput(long frame, float dx, float dy)
        {
            DeltaX = dx;
            DeltaY = dy;
        }

        public void ReceiveWorldState(float x, float y, float dx, float dy)
        {
            X = x;
            Y = y;
            DeltaX = dx;
            DeltaY = dy;
        }

        /// <summary> 应用位移：先以定点数加到逻辑，再同步到显示（显示用浮点）。 </summary>
        private void ApplyMovement(float dx, float dy)
        {
            X += dx * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
            Y += dy * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
        }
    }
}