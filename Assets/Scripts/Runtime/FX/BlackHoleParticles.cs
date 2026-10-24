using UnityEngine;

namespace Blackhole
{
    /// <summary>Ambient dust and a movement trail for the player black hole.</summary>
    public class BlackHoleParticles : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private ParticleSystem dust;
        [SerializeField] private ParticleSystem trail;
        [SerializeField] private float trailSpeedThreshold = 1.1f;
        [SerializeField] private float trailEmitFactor = 7f;

        public void Configure(Rigidbody owningBody, ParticleSystem dustSystem, ParticleSystem trailSystem)
        {
            body = owningBody;
            dust = dustSystem;
            trail = trailSystem;
        }

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }
        }

        private void Update()
        {
            if (body == null || trail == null)
            {
                return;
            }

            var velocity = body.linearVelocity;
            var planar = new Vector3(velocity.x, 0f, velocity.z);
            var speed = planar.magnitude;
            if (speed < trailSpeedThreshold)
            {
                return;
            }

            var count = Mathf.Clamp(Mathf.RoundToInt(speed * trailEmitFactor * Time.deltaTime), 0, 4);
            if (count > 0)
            {
                trail.Emit(count);
            }
        }
    }
}
