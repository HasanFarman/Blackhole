using System.Collections.Generic;
using UnityEngine;

namespace Blackhole
{
    public class BlackHoleOrbit : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private float captureRadius = 4.5f;
        [SerializeField] private float minOrbitRadius = 2.2f;
        [SerializeField] private float maxOrbitRadius = 3.8f;
        [SerializeField] private float angularSpeed = 120f;

        private readonly List<OrbitSlot> slots = new();
        private readonly List<Vector3> positions = new();

        private sealed class OrbitSlot
        {
            public Debris Debris;
            public float Radius;
            public float Angle;
        }

        public float CaptureRadius => captureRadius;
        public float MinOrbitRadius => minOrbitRadius;
        public float MaxOrbitRadius => maxOrbitRadius;
        public int OrbitingCount => slots.Count;

        private void Awake()
        {
            if (center == null)
            {
                center = transform;
            }
        }

        public void Configure(Transform orbitCenter, float capture, float minRadius, float maxRadius, float speed)
        {
            center = orbitCenter != null ? orbitCenter : transform;
            captureRadius = capture;
            minOrbitRadius = minRadius;
            maxOrbitRadius = maxRadius;
            angularSpeed = speed;
        }

        public void Capture(Debris debris)
        {
            if (debris == null || debris.Body == null || Contains(debris))
            {
                return;
            }

            var origin = center != null ? center.position : transform.position;
            var offset = debris.transform.position - origin;
            offset.y = 0f;

            var angle = offset.sqrMagnitude > 0.0001f
                ? Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg
                : 0f;
            var radius = Mathf.Clamp(offset.magnitude, minOrbitRadius, maxOrbitRadius);

            slots.Add(new OrbitSlot
            {
                Debris = debris,
                Radius = radius,
                Angle = OrbitMath.WrapDegrees(angle)
            });

            debris.EnterOrbit(radius, angle, angularSpeed);
        }

        public void Remove(Debris debris)
        {
            if (debris == null)
            {
                return;
            }

            for (var i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i].Debris == debris)
                {
                    slots.RemoveAt(i);
                }
            }
        }

        public bool TryThrow(Vector3 aimDirection, float speed)
        {
            for (var i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i].Debris == null)
                {
                    slots.RemoveAt(i);
                }
            }

            if (slots.Count == 0)
            {
                return false;
            }

            var origin = center != null ? center.position : transform.position;
            positions.Clear();
            for (var i = 0; i < slots.Count; i++)
            {
                positions.Add(slots[i].Debris.transform.position);
            }

            var index = AimRules.BestAimIndex(origin, positions, aimDirection);
            if (index < 0 || index >= slots.Count)
            {
                return false;
            }

            var chosen = slots[index].Debris;
            slots.RemoveAt(index);
            chosen.Launch(aimDirection, speed);
            return true;
        }

        private bool Contains(Debris debris)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Debris == debris)
                {
                    return true;
                }
            }

            return false;
        }

        private void FixedUpdate()
        {
            if (center == null)
            {
                return;
            }

            var step = Time.fixedDeltaTime;

            for (var i = slots.Count - 1; i >= 0; i--)
            {
                var slot = slots[i];
                if (slot.Debris == null || slot.Debris.Body == null)
                {
                    slots.RemoveAt(i);
                    continue;
                }

                slot.Angle = OrbitMath.WrapDegrees(slot.Angle + angularSpeed * step);
                var target = OrbitMath.OrbitPosition(center.position, slot.Radius, slot.Angle);
                slot.Debris.Body.MovePosition(target);
            }
        }
    }
}
