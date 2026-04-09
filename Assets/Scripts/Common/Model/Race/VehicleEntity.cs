using System;

namespace SyncSample.Common.Model.Race
{
    [Serializable]
    public class VehicleEntity
    {
        public string id;
        public string name;

        public float x;
        public float z;

        /// <summary>
        /// 车辆绕 Y 轴的朝向角，单位：度。
        /// 0 度表示朝 +Z 方向前进。
        /// </summary>
        public float rotation;

        /// <summary>
        /// 当前标量速度。大于 0 为前进，小于 0 为倒车。
        /// </summary>
        public float speed;

        private float _horizontal;
        private float _vertical;

        public const float maxForwardSpeed = 120f;
        public const float maxReverseSpeed = 20f;
        public const float acceleration = 30f;
        public const float brakeDeceleration = 70f;
        public const float naturalDeceleration = 10f;
        public const float turnSpeed = 30f;

        public VehicleEntity(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        /// <summary>
        /// 接收归一化输入并推进车辆状态。
        /// horizontal: [-1, 1]，负数左转，正数右转。
        /// vertical: [-1, 1]，负数后退/刹车，正数前进。
        /// </summary>
        public void ReceiveInput(float horizontal, float vertical)
        {
            _horizontal = Clamp(horizontal, -1f, 1f);
            _vertical = Clamp(vertical, -1f, 1f);
        }

        public void UpdateState(float deltaTime)
        {
            UpdateSpeed(_vertical, deltaTime);
            UpdateRotation(_horizontal, deltaTime);

            double radians = rotation * Math.PI / 180.0;
            float forwardX = (float)Math.Sin(radians);
            float forwardZ = (float)Math.Cos(radians);

            x += forwardX * speed * deltaTime;
            z += forwardZ * speed * deltaTime;
        }

        private void UpdateSpeed(float vertical, float deltaTime)
        {
            if (vertical > 0f)
            {
                if (speed < 0f)
                {
                    speed = MoveTowards(speed, 0f, brakeDeceleration * vertical * deltaTime);
                }
                else
                {
                    speed = Math.Min(speed + acceleration * vertical * deltaTime, maxForwardSpeed);
                }
            }
            else if (vertical < 0f)
            {
                if (speed > 0f)
                {
                    speed = MoveTowards(speed, 0f, brakeDeceleration * -vertical * deltaTime);
                }
                else
                {
                    speed = Math.Max(speed + acceleration * vertical * deltaTime, -maxReverseSpeed);
                }
            }
            else
            {
                speed = MoveTowards(speed, 0f, naturalDeceleration * deltaTime);
            }
        }

        private void UpdateRotation(float horizontal, float deltaTime)
        {
            if (Math.Abs(speed) <= 0.001f)
                return;

            if (Math.Abs(horizontal) <= 0.001f)
                return;

            float moveDirection = speed > 0f ? 1f : -1f;
            float speedFactor = Math.Min(1f, Math.Abs(speed) / Math.Max(0.001f, maxForwardSpeed));
            rotation += horizontal * moveDirection * turnSpeed * speedFactor * deltaTime;
        }

        private static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
                return target;

            return current + Math.Sign(target - current) * maxDelta;
        }

        protected static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public bool IsStateEqual(VehicleEntity other)
        {
            if (other == null)
                return false;

            const float tolerance = 0.01f;
            return string.Equals(id, other.id)
                && string.Equals(name, other.name)
                && Math.Abs(x - other.x) < tolerance
                && Math.Abs(z - other.z) < tolerance
                && Math.Abs(rotation - other.rotation) < tolerance
                && Math.Abs(speed - other.speed) < tolerance;
        }
    }
}