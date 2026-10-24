using UnityEngine;

namespace Blackhole
{
    public static class PlayerMotion
    {
        public static Vector3 CalculateVelocity(
            Vector3 currentVelocity,
            Vector2 input,
            float maxSpeed,
            float acceleration,
            float deltaTime)
        {
            var planarInput = new Vector3(input.x, 0f, input.y);
            if (planarInput.sqrMagnitude > 1f)
            {
                planarInput.Normalize();
            }

            var target = planarInput * Mathf.Max(0f, maxSpeed);
            var current = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            return Vector3.MoveTowards(current, target, Mathf.Max(0f, acceleration) * Mathf.Max(0f, deltaTime));
        }
    }
}
