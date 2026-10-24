using UnityEngine;

namespace Blackhole
{
    /// <summary>Pure menu drift + pointer lean maths, kept testable.</summary>
    public static class MenuMotion
    {
        public static Vector3 Wander(float time, float radius)
        {
            return new Vector3(
                Mathf.Sin(time * 0.21f) * radius,
                Mathf.Sin(time * 0.17f + 1.3f) * radius * 0.28f,
                Mathf.Cos(time * 0.13f) * radius * 0.55f);
        }

        public static Vector3 Lean(Vector3 anchor, Vector3 pointerPoint, float gain, float maxDistance)
        {
            var offset = pointerPoint - anchor;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            return Vector3.ClampMagnitude(offset * Mathf.Max(0f, gain), Mathf.Max(0f, maxDistance));
        }
    }
}
