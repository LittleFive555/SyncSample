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

            if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
            {
                horizontal = Clamp(horizontal, -1f, 1f);
                vertical = Clamp(vertical, -1f, 1f);

                base.ReceiveInput(horizontal, vertical);
                UpdateState(deltaTime);
                _predictedStates[frame] = CreateSnapshot();
                Logger.Log($"[{frame}] 预测 x: {x}, z: {z}, rotation: {rotation}, speed: {speed}");
                return;
            }
        }

        public void ReceiveWorldState(long frame, float x, float z, float rotation, float speed)
        {
            if (IsLocal && GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
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
                        Logger.Log($"[{frame}] 预测成功 x: {x}, z: {z}, rotation: {rotation}, speed: {speed}");
                        return;
                    }
                    else
                    {
                        Logger.Log($"[{frame}] 预测失败 x: {x}, z: {z}, rotation: {rotation}, speed: {speed}");
                    }
                    ClearPredictedStates();
                }
                else // 本地落后于服务器
                {
                    Logger.Log($"[{frame}] 本地落后于服务器，清空预测状态，直接同步服务器状态");
                    ClearPredictedStates();
                }
            }

            this.x = x;
            this.z = z;
            this.rotation = rotation;
            this.speed = speed;
            Logger.Log($"[{frame}] 收到服务器状态 x: {x}, z: {z}, rotation: {rotation}, speed: {speed}");
        }

        public void ClearPredictedStates()
        {
            _predictedStates.Clear();
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
