using System;
using SyncSample.Client.Race.Logic;
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
                // 先旋转朝向
                float currentYRotation = transform.localRotation.eulerAngles.y;
                float targetYRotation = Entity.rotation;
                float speedFactor = Math.Min(1f, Math.Abs(Entity.speed) / Math.Max(0.001f, VehicleEntity.maxForwardSpeed));
                float rotationStep = VehicleEntity.turnSpeed * speedFactor * Time.deltaTime;
                float newYRotation = Mathf.MoveTowardsAngle(currentYRotation, targetYRotation, rotationStep);
                transform.localRotation = Quaternion.Euler(0f, newYRotation, 0f);

                // 位置直接朝逻辑点插值，不沿当前车头方向推进，
                // 否则在转向过程中会持续累积横向偏差。
                Vector3 currentPosition = transform.localPosition;
                Vector3 targetPosition = new Vector3(Entity.x, currentPosition.y, Entity.z);
                var absSpeed = Mathf.Abs(Entity.speed);
                if (Vector3.Distance(currentPosition, targetPosition) > absSpeed * RaceWorldManager.Instance.FrameDeltaTime * 2) // 如果逻辑点位变化过大，直接同步
                {
                    Logger.Log($"逻辑点位变化过大，显示上直接同步 x: {Entity.x}, z: {Entity.z}");
                    SyncTransform();
                }
                else
                {
                    float moveStep = absSpeed * Time.deltaTime;
                    transform.localPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveStep);
                }
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
