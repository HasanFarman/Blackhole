using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackhole
{
    /// <summary>
    /// The menu's living black hole: wanders on a slow path, leans toward the
    /// pointer, drags its debris along and leaves a soft trail.
    /// </summary>
    public class MenuBlackHole : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private Transform[] debris;
        [SerializeField] private Camera menuCamera;
        [SerializeField] private ParticleSystem trail;
        [SerializeField] private float wanderRadius = 0.6f;
        [SerializeField] private float leanGain = 0.45f;
        [SerializeField] private float maxLean = 1.15f;
        [SerializeField] private float visualSpin = 4.5f;
        [SerializeField] private float trailEmitPerUnit = 55f;

        private Vector3 anchor;
        private Vector3 lean;
        private Vector3 leanVelocity;
        private Vector3 previousPosition;
        private float time;

        private float[] radii;
        private float[] speeds;
        private float[] phases;
        private float[] heights;

        public void Configure(Transform visualRoot, Transform[] orbitDebris, Camera camera, ParticleSystem trailSystem)
        {
            visual = visualRoot;
            debris = orbitDebris;
            menuCamera = camera;
            trail = trailSystem;
        }

        private void Awake()
        {
            anchor = transform.position;
            previousPosition = transform.position;

            if (menuCamera == null)
            {
                menuCamera = Camera.main;
            }

            BuildOrbit();
        }

        private void BuildOrbit()
        {
            if (debris == null)
            {
                debris = new Transform[0];
            }

            radii = new float[debris.Length];
            speeds = new float[debris.Length];
            phases = new float[debris.Length];
            heights = new float[debris.Length];

            for (var i = 0; i < debris.Length; i++)
            {
                var t = debris.Length <= 1 ? 0f : i / (float)(debris.Length - 1);
                radii[i] = Mathf.Lerp(2.05f, 3.55f, t) + Random.Range(-0.12f, 0.12f);
                speeds[i] = Mathf.Lerp(10f, 4.2f, t) * (i % 2 == 0 ? 1f : -1f);
                phases[i] = Random.Range(0f, 360f);
                heights[i] = Random.Range(-0.16f, 0.22f);
            }
        }

        private void Update()
        {
            var delta = Time.unscaledDeltaTime;
            time += delta;

            var wander = MenuMotion.Wander(time, wanderRadius);

            var wantedLean = Vector3.zero;
            if (TryPointerOnDisc(out var pointerPoint))
            {
                wantedLean = MenuMotion.Lean(anchor, pointerPoint, leanGain, maxLean);
            }

            lean = Vector3.SmoothDamp(lean, wantedLean, ref leanVelocity, 0.55f, 3f, delta);

            previousPosition = transform.position;
            transform.position = anchor + wander + lean;

            if (visual != null)
            {
                visual.Rotate(Vector3.up, visualSpin * delta, Space.Self);
            }

            UpdateDebris(delta);
            UpdateTrail(delta);
        }

        private bool TryPointerOnDisc(out Vector3 point)
        {
            point = Vector3.zero;
            var mouse = Mouse.current;
            if (menuCamera == null || mouse == null)
            {
                return false;
            }

            var ray = menuCamera.ScreenPointToRay(mouse.position.ReadValue());
            var plane = new Plane(Vector3.up, anchor);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }

            point = ray.GetPoint(distance);
            return true;
        }

        private void UpdateDebris(float delta)
        {
            if (debris == null)
            {
                return;
            }

            for (var i = 0; i < debris.Length; i++)
            {
                var item = debris[i];
                if (item == null)
                {
                    continue;
                }

                phases[i] += speeds[i] * delta;
                var radians = phases[i] * Mathf.Deg2Rad;
                var position = anchor + new Vector3(
                    Mathf.Cos(radians) * radii[i],
                    heights[i] + Mathf.Sin(time * 0.6f + i) * 0.05f,
                    Mathf.Sin(radians) * radii[i] * 0.9f);

                item.position = position;
                item.Rotate(new Vector3(12f, 26f, 8f) * delta, Space.Self);
            }
        }

        private void UpdateTrail(float delta)
        {
            if (trail == null || delta <= 0f)
            {
                return;
            }

            var speed = (transform.position - previousPosition).magnitude / delta;
            if (speed < 0.01f)
            {
                return;
            }

            var count = Mathf.Clamp(Mathf.RoundToInt(speed * trailEmitPerUnit * delta), 0, 3);
            if (count > 0)
            {
                trail.Emit(count);
            }
        }
    }
}
