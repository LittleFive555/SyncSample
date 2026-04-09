using SyncSample.Client.UI;
using SyncSample.Common;
using SyncSample.Common.Model.Race;
using UnityEngine;

namespace SyncSample.Client.Race.View
{
    public class Vehicle : MonoBehaviour, IInfoSource
    {
        public VehicleEntity Entity { get; private set; }

        public string Name => Entity.name;

        public Vector2 LogicPosition => new Vector2(Entity.x, Entity.z);

        public Vector2 ViewPosition => new Vector2(transform.localPosition.x, transform.localPosition.z);

        public void Init(VehicleEntity vehicleEntity)
        {
            Entity = vehicleEntity;
            UIInfo.Instance.RegisterPos(this);
            SyncTransform();
        }

        private void Update()
        {
            if (Entity == null)
                return;

            if (GlobalSwitch.Instance.ClientInterpolation)
            {
                float moveStep = VehicleEntity.maxForwardSpeed * Time.deltaTime;
                Vector3 onlyX = Vector3.MoveTowards(
                    transform.localPosition,
                    new Vector3(Entity.x, transform.localPosition.y, transform.localPosition.z),
                    moveStep);
                Vector3 onlyZ = Vector3.MoveTowards(
                    transform.localPosition,
                    new Vector3(transform.localPosition.x, transform.localPosition.y, Entity.z),
                    moveStep);
                transform.localPosition = new Vector3(onlyX.x, transform.localPosition.y, onlyZ.z);

                // 插值旋转
                float currentYRotation = transform.localRotation.eulerAngles.y;
                float targetYRotation = Entity.rotation;
                float rotationStep = VehicleEntity.turnSpeed * Time.deltaTime;
                float newYRotation = Mathf.MoveTowardsAngle(currentYRotation, targetYRotation, rotationStep);
                transform.localRotation = Quaternion.Euler(0f, newYRotation, 0f);
            }
            else
            {
                SyncTransform();
            }
        }

        private void SyncTransform()
        {
            transform.localPosition = new Vector3(Entity.x, 0f, Entity.z);
            transform.localRotation = Quaternion.Euler(0f, Entity.rotation, 0f);
        }

        private void OnDestroy()
        {
            if (UIInfo.Instance != null)
                UIInfo.Instance.UnregisterPos(this);
        }
    }
}
