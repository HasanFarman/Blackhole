using UnityEngine;

namespace Blackhole
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Range(60f, 70f)] private float pitch = 65f;
        [SerializeField] private float distance = 14f;
        [SerializeField] private float followSharpness = 8f;
        [SerializeField] private float shakeStrength = 0.18f;

        private Vector3 smoothVelocity;
        private float shakeTimer;
        private float shakeTimerDuration;

        public static CameraFollow Instance { get; private set; }
        public float Pitch => pitch;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Configure(Transform followTarget, float pitchDegrees, float followDistance, float sharpness)
        {
            target = followTarget;
            pitch = Mathf.Clamp(pitchDegrees, 60f, 70f);
            distance = Mathf.Max(0.1f, followDistance);
            followSharpness = Mathf.Max(0.01f, sharpness);
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            smoothVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.position = DesiredPosition();
        }

        public void Shake(float strength, float duration)
        {
            shakeStrength = Mathf.Max(shakeStrength, strength);
            shakeTimerDuration = Mathf.Max(0.01f, duration);
            shakeTimer = shakeTimerDuration;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var smoothTime = 1f / Mathf.Max(0.01f, followSharpness);
            var desired = DesiredPosition();
            var position = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, smoothTime);

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.unscaledDeltaTime;
                var falloff = Mathf.Clamp01(shakeTimer / shakeTimerDuration);
                position += Random.insideUnitSphere * (shakeStrength * falloff);
            }

            transform.position = position;
            transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private Vector3 DesiredPosition()
        {
            var rotation = Quaternion.Euler(pitch, 0f, 0f);
            return target.position - rotation * Vector3.forward * distance;
        }
    }
}
