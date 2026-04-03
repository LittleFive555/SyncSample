using SyncSample.Client.Gameplay.World.Logic;
using SyncSample.Common;

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
            if (!IsLocal)
                return;

            DeltaX = dx;
            DeltaY = dy;
            
            if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                X += DeltaX * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
                Y += DeltaY * MoveSpeed * SyncStateWorldManager.Instance.FrameDeltaTime;
            }
        }

        public void ReceiveWorldState(float x, float y, float dx, float dy)
        {
            X = x;
            Y = y;
            DeltaX = dx;
            DeltaY = dy;
        }
    }
}