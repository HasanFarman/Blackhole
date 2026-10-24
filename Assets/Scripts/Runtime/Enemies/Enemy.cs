using System;
using System.Collections;
using UnityEngine;

namespace Blackhole
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private Collider hitCollider;
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private float seekAcceleration = 12f;
        [SerializeField] private float maxSpeed = 4.5f;
        [SerializeField] private float wobbleStrength = 0.45f;
        [SerializeField] private float spinTorque = 8f;
        [SerializeField] private float lungeIntervalMin = 2.5f;
        [SerializeField] private float lungeIntervalMax = 5.5f;
        [SerializeField] private float lungeStrength = 4f;
        [SerializeField] private float deathFlashSeconds = 0.06f;

        private Transform target;
        private DebrisField debrisField;
        private float phase;
        private float lungeTimer;
        private bool dead;

        public event Action<Enemy> Died;
        public bool IsDead => dead;

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (hitCollider == null)
            {
                hitCollider = GetComponent<Collider>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<Renderer>();
            }

            if (body != null)
            {
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezePositionY;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        public void Initialize(Transform chaseTarget, DebrisField field)
        {
            target = chaseTarget;
            debrisField = field;
            phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            lungeTimer = UnityEngine.Random.Range(lungeIntervalMin, lungeIntervalMax);
        }

        private void FixedUpdate()
        {
            if (dead || target == null || body == null)
            {
                return;
            }

            var direction = EnemyMotion.SeekDirection(
                transform.position,
                target.position,
                Time.time,
                phase,
                wobbleStrength);

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            body.AddForce(direction * seekAcceleration, ForceMode.Acceleration);

            var velocity = body.linearVelocity;
            var planar = new Vector3(velocity.x, 0f, velocity.z);
            if (planar.magnitude > maxSpeed)
            {
                planar = planar.normalized * maxSpeed;
                body.linearVelocity = new Vector3(planar.x, velocity.y, planar.z);
            }

            if (body.angularVelocity.magnitude < 8f)
            {
                body.AddTorque(Vector3.up * spinTorque, ForceMode.Acceleration);
            }

            lungeTimer -= Time.fixedDeltaTime;
            if (lungeTimer <= 0f)
            {
                body.AddForce(direction * lungeStrength, ForceMode.VelocityChange);
                lungeTimer = UnityEngine.Random.Range(lungeIntervalMin, lungeIntervalMax);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (dead)
            {
                return;
            }

            var player = collision.collider.GetComponentInParent<BlackHoleController>();
            if (player != null && !player.IsDead)
            {
                player.Die();
            }
        }

        public void TakeHit()
        {
            Die();
        }

        public void Die()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            FlashWhite();
            Died?.Invoke(this);
            CameraFollow.Instance?.Shake(0.14f, 0.1f);
            StartCoroutine(FinishDeath());
        }

        private IEnumerator FinishDeath()
        {
            yield return new WaitForSeconds(deathFlashSeconds);
            debrisField?.SpawnBurst(transform.position, 1);
            Destroy(gameObject);
        }

        private void FlashWhite()
        {
            if (meshRenderer == null)
            {
                return;
            }

            var material = meshRenderer.material;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", Color.white);
            }
            else if (material.HasProperty("_Color"))
            {
                material.color = Color.white;
            }
        }
    }
}
