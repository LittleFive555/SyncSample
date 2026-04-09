using System.Collections.Generic;
using SyncSample.Common;
using SyncSample.Common.Model.Race;

namespace SyncSample.Client.Race.Logic
{
    public class ClientVehicleEntity : VehicleEntity
    {
        public bool IsLocal { get; private set; }

        private readonly SortedList<long, VehicleEntity> _predictedStates = new SortedList<long, VehicleEntity>();

        public ClientVehicleEntity(string id, string name, bool isLocal)
            : base(id, name)
        {
            IsLocal = isLocal;
        }

        public void ReceiveInput(long frame, float horizontal, float vertical, float deltaTime)
        {
            if (!IsLocal)
                return;

            horizontal = Clamp(horizontal, -1f, 1f);
            vertical = Clamp(vertical, -1f, 1f);

            if (GlobalSwitch.Instance != null && GlobalSwitch.Instance.StateSyncSwitch != null && GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                base.ReceiveInput(horizontal, vertical);
                _predictedStates[frame] = CreateSnapshot();
                return;
            }

            base.ReceiveInput(horizontal, vertical);
        }

        public void ReceiveWorldState(long frame, float x, float z, float rotation, float speed)
        {
            if (IsLocal && GlobalSwitch.Instance != null && GlobalSwitch.Instance.StateSyncSwitch != null && GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                if (_predictedStates.TryGetValue(frame, out var predictedState))
                {
                    var serverState = new VehicleEntity(id, name)
                    {
                        x = x,
                        z = z,
                        rotation = rotation,
                        speed = speed
                    };

                    if (predictedState.IsStateEqual(serverState))
                    {
                        _predictedStates.Remove(frame);
                        return;
                    }

                    _predictedStates.Clear();
                }
            }

            this.x = x;
            this.z = z;
            this.rotation = rotation;
            this.speed = speed;
        }

        private VehicleEntity CreateSnapshot()
        {
            return new VehicleEntity(id, name)
            {
                x = x,
                z = z,
                rotation = rotation,
                speed = speed
            };
        }
    }
}
