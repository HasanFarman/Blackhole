using UnityEngine;

namespace Blackhole
{
    public class Debris : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Transform visual;
        [SerializeField] private float spinDegreesPerSecond = 180f;
        [SerializeField] private float thrownDuration = 1.8f;
        [SerializeField] private float recaptureCooldownSeconds = 0.35f;

        private BlackHoleOrbit orbit;
        private Transform player;
        private DebrisField field;
        private DebrisState state = DebrisState.Loose;
        private float thrownTimer;
        private float recaptureTimer;

        public DebrisState State => state;
        public Rigidbody Body => body != null ? body : body = GetComponent<Rigidbody>();

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (visual == null && transform.childCount > 0)
            {
                visual = transform.GetChild(0);
            }
        }

        public void Configure(BlackHoleOrbit owningOrbit, Transform owner, DebrisField owningField)
        {
            orbit = owningOrbit;
            player = owner;
            field = owningField;
            state = DebrisState.Loose;
            thrownTimer = 0f;
            recaptureTimer = 0f;

            if (Body == null)
            {
                return;
            }

            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.isKinematic = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void ApplyGravity(Vector3 center, float acceleration)
        {
            if (state != DebrisState.Loose || recaptureTimer > 0f || Body == null)
            {
                return;
            }

            var offset = center - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f)
            {
                return;
            }

            body.AddForce(offset.normalized * acceleration, ForceMode.Acceleration);
        }

        public void EnterOrbit(float radius, float angleDegrees, float angularSpeed)
        {
            state = DebrisState.Orbiting;
            thrownTimer = 0f;
            recaptureTimer = 0f;

            if (Body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }
        }

        public void Launch(Vector3 direction, float speed)
        {
            orbit?.Remove(this);
            state = DebrisState.Thrown;
            thrownTimer = thrownDuration;
            recaptureTimer = 0f;

            if (Body == null)
            {
                return;
            }

            var flatDirection = new Vector3(direction.x, 0f, direction.z);
            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                flatDirection = Vector3.forward;
            }

            body.isKinematic = false;
            body.linearVelocity = flatDirection.normalized * speed;
            body.angularVelocity = Random.onUnitSphere * 12f;
        }

        public void ReturnToLoose()
        {
            if (state == DebrisState.Loose && recaptureTimer > 0f)
            {
                return;
            }

            state = DebrisState.Loose;
            thrownTimer = 0f;
            recaptureTimer = recaptureCooldownSeconds;

            if (Body != null)
            {
                body.isKinematic = false;
            }
        }

        private void FixedUpdate()
        {
            var step = Time.fixedDeltaTime;

            if (recaptureTimer > 0f)
            {
                recaptureTimer -= step;
            }

            if (state == DebrisState.Thrown)
            {
                thrownTimer -= step;
                if (thrownTimer <= 0f)
                {
                    ReturnToLoose();
                }
            }
            else if (state == DebrisState.Loose &&
                     recaptureTimer <= 0f &&
                     orbit != null &&
                     player != null)
            {
                var offset = transform.position - player.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= orbit.CaptureRadius * orbit.CaptureRadius)
                {
                    orbit.Capture(this);
                }
            }

            if (visual != null)
            {
                visual.Rotate(Vector3.up, spinDegreesPerSecond * step, Space.Self);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (state != DebrisState.Thrown)
            {
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead)
            {
                return;
            }

            damageable.TakeHit();
            ReturnToLoose();
        }

        private void OnDestroy()
        {
            orbit?.Remove(this);
            field?.ReportDestroyed(this);
        }
    }
}
