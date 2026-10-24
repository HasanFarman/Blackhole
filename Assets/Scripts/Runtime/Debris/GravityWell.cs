using UnityEngine;

namespace Blackhole
{
    public class GravityWell : MonoBehaviour
    {
        [SerializeField] private float attractRadius = 10f;
        [SerializeField] private float attractionAcceleration = 20f;
        [SerializeField] private LayerMask debrisMask;

        private readonly Collider[] hits = new Collider[128];

        public float AttractRadius => attractRadius;

        public void Configure(float radius, float acceleration, LayerMask mask)
        {
            attractRadius = radius;
            attractionAcceleration = acceleration;
            debrisMask = mask;
        }

        private void FixedUpdate()
        {
            if (debrisMask.value == 0)
            {
                return;
            }

            var count = Physics.OverlapSphereNonAlloc(
                transform.position,
                attractRadius,
                hits,
                debrisMask,
                QueryTriggerInteraction.Collide);

            for (var i = 0; i < count; i++)
            {
                var collider = hits[i];
                if (collider == null)
                {
                    continue;
                }

                var debris = collider.GetComponent<Debris>();
                if (debris == null)
                {
                    debris = collider.GetComponentInParent<Debris>();
                }

                debris?.ApplyGravity(transform.position, attractionAcceleration);
            }
        }
    }
}
