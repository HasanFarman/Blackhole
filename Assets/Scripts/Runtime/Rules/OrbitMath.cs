using UnityEngine;

namespace Blackhole
{
    public static class OrbitMath
    {
        public static Vector3 OrbitPosition(Vector3 center, float radius, float angleDegrees)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
        }

        public static float WrapDegrees(float degrees)
        {
            return Mathf.Repeat(degrees, 360f);
        }
    }
}
