using System;
using System.Collections.Generic;
using SyncSample.Common.Model.Race;

namespace SyncSample.Client.Race.Logic
{
    public class VehicleManager
    {
        private static VehicleManager _instance;
        public static VehicleManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new VehicleManager();
                return _instance;
            }
        }

        private readonly Dictionary<string, ClientVehicleEntity> _vehicleEntities = new Dictionary<string, ClientVehicleEntity>();

        public string SelfId;

        public Action<VehicleEntity> OnVehicleCreated;
        public Action<VehicleEntity> OnVehicleRemoved;

        public ClientVehicleEntity EnsureVehicle(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            if (_vehicleEntities.TryGetValue(id, out var vehicleEntity))
                return vehicleEntity;

            vehicleEntity = new ClientVehicleEntity(id, displayName, SelfId == id);

            _vehicleEntities[id] = vehicleEntity;
            OnVehicleCreated?.Invoke(vehicleEntity);
            return vehicleEntity;
        }

        public void RemoveVehicle(string id)
        {
            if (_vehicleEntities.TryGetValue(id, out var vehicleEntity) && vehicleEntity != null)
            {
                _vehicleEntities.Remove(id);
                OnVehicleRemoved?.Invoke(vehicleEntity);
            }
        }

        public ClientVehicleEntity GetVehicle(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            _vehicleEntities.TryGetValue(id, out var vehicleEntity);
            return vehicleEntity;
        }

        /// <summary>
        /// 客户端预测使用。
        /// </summary>
        public void ReceiveInput(long frame, float horizontal, float vertical, float deltaTime)
        {
            if (string.IsNullOrEmpty(SelfId) || !_vehicleEntities.TryGetValue(SelfId, out var entity) || entity == null)
                return;

            entity.ReceiveInput(frame, horizontal, vertical, deltaTime);
        }

        /// <summary>
        /// 根据外部同步数据创建缺失车辆并覆盖状态。
        /// </summary>
        public void ApplyServerWorldState(long frame, VehicleEntity[] vehicles)
        {
            if (vehicles == null)
                return;

            for (int i = 0; i < vehicles.Length; i++)
            {
                var e = vehicles[i];
                if (e == null || string.IsNullOrEmpty(e.id))
                    continue;

                string displayName = string.IsNullOrEmpty(e.name) ? e.id : e.name;
                var vehicleEntity = EnsureVehicle(e.id, displayName);
                if (vehicleEntity == null)
                    continue;

                vehicleEntity.name = e.name;
                vehicleEntity.ReceiveWorldState(frame, e.x, e.z, e.rotation, e.speed);
            }
        }
    }
}
