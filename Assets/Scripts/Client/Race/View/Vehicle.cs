using System;
using System.Collections.Generic;
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

        public Vector2 ViewPosition => new Vector2(transform.position.x, transform.position.z);

        private VehicleEntity _lastVehicleState;
        private VehicleEntity _nextVehicleState;

        private float _visualTimer = 0;

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
                _visualTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_visualTimer / RaceWorldManager.Instance.FrameDeltaTime);

                // 先旋转朝向，采用线性插值，不按原速度推进
                // 线性插值而不是MoveTowardsAngle
                float lerpYRotation = Mathf.LerpAngle(_lastVehicleState.rotation, _nextVehicleState.rotation, t);
                transform.rotation = Quaternion.Euler(0f, lerpYRotation, 0f);

                // 位置直接朝逻辑点插值，不沿当前车头方向推进，
                // 否则在转向过程中会持续累积横向偏差。
                var lerpLogicPosition = InterpolateLogicPosition(_lastVehicleState, _nextVehicleState, t, RaceWorldManager.Instance.FrameDeltaTime);
                transform.position = new Vector3(lerpLogicPosition.x, transform.position.y, lerpLogicPosition.y);

                var newLogicPosition = new Vector2(Entity.x, Entity.z);
                if (Vector2.Distance(new Vector2(_nextVehicleState.x, _nextVehicleState.z), newLogicPosition) > 0.01f)
                {
                    _lastVehicleState = _nextVehicleState;
                    _nextVehicleState = Entity.Clone();
                    _visualTimer = 0;
                }
            }
            else
            {
                SyncTransform();
            }
        }

        private void SyncTransform()
        {
            transform.position = new Vector3(Entity.x, 0f, Entity.z);
            transform.rotation = Quaternion.Euler(0f, Entity.rotation, 0f);
            _lastVehicleState = _nextVehicleState = Entity.Clone();
        }

        private static Vector2 InterpolateLogicPosition(VehicleEntity lastState, VehicleEntity nextState, float normalizedTime, float duration)
        {
            var start = new Vector2(lastState.x, lastState.z);
            var end = new Vector2(nextState.x, nextState.z);
            var displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.0001f || duration <= 0.0001f)
                return end;

            var direction = displacement / distance;
            float startSpeed = GetProjectedSpeed(lastState, direction);
            float endSpeed = GetProjectedSpeed(nextState, direction);

            // 保持由两端速度决定的加速度，同时整体平移速度曲线，让总位移精确落到目标点。
            float acceleration = (endSpeed - startSpeed) / duration;
            float speedOffset = distance / duration - (startSpeed + endSpeed) * 0.5f;
            float elapsed = Mathf.Clamp01(normalizedTime) * duration;
            float traveledDistance = (startSpeed + speedOffset) * elapsed + 0.5f * acceleration * elapsed * elapsed;
            return start + direction * Mathf.Clamp(traveledDistance, 0f, distance);
        }

        private static float GetProjectedSpeed(VehicleEntity state, Vector2 direction)
        {
            float radians = state.rotation * Mathf.Deg2Rad;
            var forward = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
            return Vector2.Dot(forward * state.speed, direction);
        }

        private void OnDestroy()
        {
            if (UIInfo.Instance != null)
                UIInfo.Instance.UnregisterPos(this);
        }
    }
}
