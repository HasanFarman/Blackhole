using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackhole
{
    /// <summary>Low, off-axis menu camera with a slow push-in, breathing and mouse parallax.</summary>
    [RequireComponent(typeof(Camera))]
    public class MenuCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 focus = new Vector3(2.9f, 1.55f, 0f);
        [SerializeField] private Vector3 framingOffset = new Vector3(-2.4f, 0.15f, 0f);
        [SerializeField] private float pitch = 21f;
        [SerializeField] private float yaw = -9f;
        [SerializeField] private float distance = 16.5f;
        [SerializeField] private float parallax = 0.9f;

        private float introProgress = 1f;
        private float introFrom = 16.5f;
        private float introDuration = 2.6f;
        private Vector3 smoothOffset;

        public void Configure(Vector3 focusPoint, float pitchDegrees, float yawDegrees, float viewDistance, float parallaxAmount)
        {
            focus = focusPoint;
            pitch = pitchDegrees;
            yaw = yawDegrees;
            distance = Mathf.Max(1f, viewDistance);
            parallax = parallaxAmount;
        }

        public void BeginIntro(float fromDistance, float duration)
        {
            introFrom = Mathf.Max(distance, fromDistance);
            introDuration = Mathf.Max(0.01f, duration);
            introProgress = 0f;
        }

        private void Awake()
        {
            Snap();
        }

        public void Snap()
        {
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var lookTarget = focus + framingOffset;
            transform.rotation = rotation;
            transform.position = lookTarget - rotation * Vector3.forward * distance;
            smoothOffset = Vector3.zero;
        }

        private void LateUpdate()
        {
            var delta = Time.unscaledDeltaTime;
            introProgress = Mathf.Clamp01(introProgress + delta / introDuration);

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var mouse = MouseOffset();

            var wantedOffset = rotation * new Vector3(mouse.x * parallax, mouse.y * parallax * 0.45f, 0f);
            smoothOffset = Vector3.Lerp(smoothOffset, wantedOffset, 1f - Mathf.Exp(-3.2f * delta));

            var breath = new Vector3(
                Mathf.Sin(Time.unscaledTime * 0.17f) * 0.22f,
                Mathf.Sin(Time.unscaledTime * 0.23f + 0.8f) * 0.1f,
                0f);

            var eased = EaseOutCubic(introProgress);
            var currentDistance = Mathf.Lerp(introFrom, distance, eased);
            var lookTarget = focus + framingOffset + smoothOffset + breath;

            transform.rotation = rotation;
            transform.position = lookTarget - rotation * Vector3.forward * currentDistance;
        }

        private static Vector2 MouseOffset()
        {
            var mouse = Mouse.current;
            if (mouse == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector2.zero;
            }

            var position = mouse.position.ReadValue();
            var offset = new Vector2(
                position.x / Screen.width - 0.5f,
                position.y / Screen.height - 0.5f) * 2f;

            return Vector2.ClampMagnitude(offset, 1f);
        }

        private static float EaseOutCubic(float t)
        {
            var inverse = 1f - Mathf.Clamp01(t);
            return 1f - inverse * inverse * inverse;
        }
    }
}
