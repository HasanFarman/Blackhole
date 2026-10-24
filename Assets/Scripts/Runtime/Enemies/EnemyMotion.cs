using UnityEngine;

namespace Blackhole
{
    public static class EnemyMotion
    {
        public static Vector3 SeekDirection(
            Vector3 enemyPosition,
            Vector3 targetPosition,
            float time,
            float phase,
            float wobbleStrength)
        {
            var offset = targetPosition - enemyPosition;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            var direction = offset.normalized;
            var perpendicular = new Vector3(-direction.z, 0f, direction.x);
            var wobble = Mathf.Sin(time * 2f + phase) * wobbleStrength;
            return (direction + perpendicular * wobble).normalized;
        }
    }
}
