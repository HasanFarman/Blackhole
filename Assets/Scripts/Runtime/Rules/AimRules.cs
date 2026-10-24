using System.Collections.Generic;
using UnityEngine;

namespace Blackhole
{
    public static class AimRules
    {
        public static int BestAimIndex(Vector3 origin, IReadOnlyList<Vector3> positions, Vector3 aimDirection)
        {
            if (positions == null || positions.Count == 0)
            {
                return -1;
            }

            var flatDirection = new Vector3(aimDirection.x, 0f, aimDirection.z);
            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                return -1;
            }

            flatDirection.Normalize();

            var bestIndex = -1;
            var bestScore = float.NegativeInfinity;

            for (var i = 0; i < positions.Count; i++)
            {
                var offset = positions[i] - origin;
                var flatOffset = new Vector3(offset.x, 0f, offset.z);
                if (flatOffset.sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                var score = Vector3.Dot(flatOffset.normalized, flatDirection);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        public static bool TryGroundPoint(Ray ray, float planeY, out Vector3 point)
        {
            var plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));
            if (plane.Raycast(ray, out var distance) && distance >= 0f)
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = default;
            return false;
        }
    }
}
