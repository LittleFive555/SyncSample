using UnityEngine;

namespace SyncSample.Client
{
    public class FollowCamera : MonoBehaviour
    {
        public float height = 30f;
        public float baseDistance = 50f;
        public float maxExtraDistance = 3f;
        public float lookAtHeight = 1.6f;
        public float baseLookAhead = 6f;
        public float maxExtraLookAhead = 3f;
        public Transform target;

        public float motionSmoothSpeed = 8f;
        public float forwardSmoothSpeed = 12f;
        public float positionSmoothTime = 0.18f;
        public float rotationSmoothSpeed = 9f;
        public float speedForMaxEffects = 40f;
        public float maxTurnOffset = 0.75f;
        public float turnOffsetSmoothSpeed = 6f;
        public float maxTurnRateForEffects = 100f;
        public float maxRollAngle = 2f;

        public float baseFov = 60f;
        public float maxFov = 68f;
        public float fovSmoothSpeed = 4f;

        private Camera _cachedCamera;
        private Transform _trackedTarget;
        private Vector3 _positionVelocity;
        private Vector3 _lastTargetPosition;
        private float _lastTargetYaw;
        private Vector3 _smoothedPlanarForward = Vector3.forward;
        private float _smoothedTargetSpeed;
        private float _smoothedTurnRate;
        private float _currentTurnOffset;

        private void LateUpdate()
        {
            if (target == null)
                return;

            EnsureTargetTrackingState();
            UpdateTargetMotion(Time.deltaTime);
            UpdateCamera(Time.deltaTime);
        }

        private void OnValidate()
        {
            ResetFollowState();
            CacheCamera();

            if (target == null)
                return;

            EnsureTargetTrackingState();
            UpdateTargetMotion(0f);
            UpdateCamera(0f);
        }

        private void UpdateCamera(float deltaTime)
        {
            Vector3 rigForward = GetSafePlanarForward(_smoothedPlanarForward, target.forward);
            Vector3 rigRight = Vector3.Cross(Vector3.up, rigForward).normalized;

            float speed01 = GetNormalizedValue(_smoothedTargetSpeed, speedForMaxEffects);
            float normalizedTurnRate = GetSignedNormalizedValue(_smoothedTurnRate, maxTurnRateForEffects);
            float targetTurnOffset = -normalizedTurnRate * maxTurnOffset;

            if (deltaTime <= 0f)
            {
                _currentTurnOffset = targetTurnOffset;
            }
            else
            {
                _currentTurnOffset = Mathf.Lerp(_currentTurnOffset, targetTurnOffset, GetLerpFactor(turnOffsetSmoothSpeed, deltaTime));
            }

            float distance = baseDistance + speed01 * maxExtraDistance;
            float lookAhead = baseLookAhead + speed01 * maxExtraLookAhead;

            Vector3 desiredPosition =
                target.position
                - rigForward * distance
                + Vector3.up * height
                + rigRight * _currentTurnOffset;

            transform.position = deltaTime <= 0f
                ? desiredPosition
                : Vector3.SmoothDamp(transform.position, desiredPosition, ref _positionVelocity, positionSmoothTime);

            Vector3 lookTarget =
                target.position +
                Vector3.up * lookAtHeight +
                rigForward * lookAhead;
            Vector3 lookDirection = lookTarget - transform.position;
            if (lookDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            desiredRotation *= Quaternion.AngleAxis(-normalizedTurnRate * maxRollAngle, Vector3.forward);

            float rotationLerpT = deltaTime <= 0f ? 1f : GetLerpFactor(rotationSmoothSpeed, deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerpT);

            UpdateFieldOfView(speed01, deltaTime);
        }

        private void EnsureTargetTrackingState()
        {
            CacheCamera();

            if (_trackedTarget == target)
                return;

            _trackedTarget = target;
            _lastTargetPosition = target.position;
            _lastTargetYaw = target.eulerAngles.y;
            _smoothedPlanarForward = GetSafePlanarForward(ProjectOnPlane(target.forward), Vector3.forward);
            _smoothedTargetSpeed = 0f;
            _smoothedTurnRate = 0f;
        }

        private void UpdateTargetMotion(float deltaTime)
        {
            Vector3 targetForward = GetSafePlanarForward(ProjectOnPlane(target.forward), Vector3.forward);

            if (deltaTime <= 0f)
            {
                _lastTargetPosition = target.position;
                _lastTargetYaw = target.eulerAngles.y;
                _smoothedPlanarForward = targetForward;
                _smoothedTargetSpeed = 0f;
                _smoothedTurnRate = 0f;
                return;
            }

            Vector3 targetDelta = target.position - _lastTargetPosition;
            targetDelta.y = 0f;
            Vector3 targetVelocity = targetDelta / deltaTime;
            float targetSpeed = targetVelocity.magnitude;

            float yaw = target.eulerAngles.y;
            float turnRate = Mathf.DeltaAngle(_lastTargetYaw, yaw) / deltaTime;
            float motionLerpT = GetLerpFactor(motionSmoothSpeed, deltaTime);
            float forwardLerpT = GetLerpFactor(forwardSmoothSpeed, deltaTime);

            _smoothedPlanarForward = Vector3.Slerp(_smoothedPlanarForward, targetForward, forwardLerpT).normalized;
            _smoothedTargetSpeed = Mathf.Lerp(_smoothedTargetSpeed, targetSpeed, motionLerpT);
            _smoothedTurnRate = Mathf.Lerp(_smoothedTurnRate, turnRate, motionLerpT);

            _lastTargetPosition = target.position;
            _lastTargetYaw = yaw;
        }

        private void ResetFollowState()
        {
            _positionVelocity = Vector3.zero;
            _trackedTarget = null;
            _lastTargetPosition = Vector3.zero;
            _lastTargetYaw = 0f;
            _smoothedPlanarForward = Vector3.forward;
            _smoothedTargetSpeed = 0f;
            _smoothedTurnRate = 0f;
            _currentTurnOffset = 0f;
        }

        private void UpdateFieldOfView(float speed01, float deltaTime)
        {
            CacheCamera();
            if (_cachedCamera == null)
                return;

            float desiredFov = Mathf.Lerp(baseFov, maxFov, speed01);
            if (deltaTime <= 0f)
            {
                _cachedCamera.fieldOfView = desiredFov;
                return;
            }

            _cachedCamera.fieldOfView = Mathf.Lerp(_cachedCamera.fieldOfView, desiredFov, GetLerpFactor(fovSmoothSpeed, deltaTime));
        }

        private void CacheCamera()
        {
            if (_cachedCamera == null)
                _cachedCamera = GetComponent<Camera>();
        }

        private static Vector3 ProjectOnPlane(Vector3 value)
        {
            return Vector3.ProjectOnPlane(value, Vector3.up);
        }

        private static Vector3 GetSafePlanarForward(Vector3 value, Vector3 fallback)
        {
            Vector3 planarValue = ProjectOnPlane(value);
            if (planarValue.sqrMagnitude > 0.0001f)
                return planarValue.normalized;

            Vector3 planarFallback = ProjectOnPlane(fallback);
            if (planarFallback.sqrMagnitude > 0.0001f)
                return planarFallback.normalized;

            return Vector3.forward;
        }

        private static float GetSignedNormalizedValue(float value, float maxAbsValue)
        {
            if (maxAbsValue <= 0f)
                return 0f;

            return Mathf.Clamp(value / maxAbsValue, -1f, 1f);
        }

        private static float GetNormalizedValue(float value, float maxValue)
        {
            if (maxValue <= 0f)
                return 0f;

            return Mathf.Clamp01(value / maxValue);
        }

        private static float GetLerpFactor(float speed, float deltaTime)
        {
            if (speed <= 0f)
                return 1f;

            return 1f - Mathf.Exp(-speed * deltaTime);
        }
    }
}
