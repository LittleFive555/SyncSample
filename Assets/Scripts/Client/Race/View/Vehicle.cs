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

        private long _nextSyncFrame;

        private float _lastYRotation;
        private float _nextYRotation;
        private Vector2 _lastLogicPosition;
        private Vector2 _nextLogicPosition;

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
                float lerpYRotation = Mathf.LerpAngle(_lastYRotation, _nextYRotation, t);
                transform.rotation = Quaternion.Euler(0f, lerpYRotation, 0f);

                // 位置直接朝逻辑点插值，不沿当前车头方向推进，
                // 否则在转向过程中会持续累积横向偏差。

                var lerpLogicPosition = Vector3.Lerp(_lastLogicPosition, _nextLogicPosition, t);
                transform.position = new Vector3(lerpLogicPosition.x, transform.position.y, lerpLogicPosition.y);

                var targetYRotation = Entity.rotation;
                var newLogicPosition = new Vector2(Entity.x, Entity.z);
                if (Vector2.Distance(_nextLogicPosition, newLogicPosition) > 0.01f)
                {
                    _lastYRotation = lerpYRotation;
                    _lastLogicPosition = lerpLogicPosition;
                    _nextYRotation = targetYRotation;
                    _nextLogicPosition = newLogicPosition;
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
            
            if (GlobalSwitch.Instance.StateSyncSwitch.ClientPrediction)
                _nextSyncFrame = RaceWorldManager.Instance.LocalFrame + 1;
            else
                _nextSyncFrame = RaceWorldManager.Instance.ServerFrame;
            _lastYRotation = _nextYRotation = Entity.rotation;
            _lastLogicPosition = _nextLogicPosition = new Vector3(Entity.x, Entity.z);
        }

        private void OnDestroy()
        {
            if (UIInfo.Instance != null)
                UIInfo.Instance.UnregisterPos(this);
        }
    }
}
