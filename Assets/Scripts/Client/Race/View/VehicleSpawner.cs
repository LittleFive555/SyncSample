using System.Collections.Generic;
using SyncSample.Common.Model.Race;
using UnityEngine;
using Cinemachine;
using SyncSample.Client.Race.Logic;

namespace SyncSample.Client.Race.View
{
    public class VehicleSpawner : MonoBehaviour
    {
        [SerializeField] private Vehicle vehiclePrefab;
        [SerializeField] private CinemachineVirtualCamera cameraFollow;

        private static VehicleSpawner _instance;
        public static VehicleSpawner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<VehicleSpawner>();
                return _instance;
            }
        }

        private readonly Dictionary<string, Vehicle> _vehicleObjects = new Dictionary<string, Vehicle>();

        public void EnsureVehicle(ClientVehicleEntity vehicleEntity)
        {
            if (vehicleEntity == null || string.IsNullOrEmpty(vehicleEntity.id))
                return;

            if (_vehicleObjects.ContainsKey(vehicleEntity.id))
                return;

            var vehicle = Instantiate(vehiclePrefab);
            vehicle.gameObject.name = vehicleEntity.name;
            vehicle.transform.SetParent(transform);
            vehicle.Init(vehicleEntity);

            if (vehicleEntity.IsLocal)
            {
                var cameraFollowInstance = Instantiate(cameraFollow);
                cameraFollowInstance.LookAt = vehicle.transform;
                cameraFollowInstance.Follow = vehicle.transform;
            }

            _vehicleObjects[vehicleEntity.id] = vehicle;
        }

        public void RemoveVehicle(VehicleEntity vehicleEntity)
        {
            if (vehicleEntity == null || string.IsNullOrEmpty(vehicleEntity.id))
                return;

            if (!_vehicleObjects.TryGetValue(vehicleEntity.id, out var vehicle) || vehicle == null)
                return;

            Destroy(vehicle.gameObject);
            _vehicleObjects.Remove(vehicleEntity.id);
        }
    }
}
