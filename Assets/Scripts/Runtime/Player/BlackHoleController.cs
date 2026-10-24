using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackhole
{
    public class BlackHoleController : MonoBehaviour
    {
        [SerializeField] private BlackHoleOrbit orbit;
        [SerializeField] private Rigidbody body;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float throwSpeed = 20f;
        [SerializeField] private float throwCooldown = 0.12f;
        [SerializeField] private float aimDeadZone = 1.5f;

        private Vector2 moveInput;
        private float nextThrowTime;
        private bool dead;

        public event Action Died;
        public bool IsDead => dead;

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (orbit == null)
            {
                orbit = GetComponent<BlackHoleOrbit>();
            }
        }

        public void Configure(
            BlackHoleOrbit owningOrbit,
            Rigidbody owningBody,
            Camera camera,
            float speed = 7f,
            float accel = 40f,
            float launchSpeed = 20f,
            float cooldown = 0.12f,
            float deadZone = 1.5f)
        {
            orbit = owningOrbit;
            body = owningBody;
            aimCamera = camera;
            moveSpeed = speed;
            acceleration = accel;
            throwSpeed = launchSpeed;
            throwCooldown = cooldown;
            aimDeadZone = deadZone;
        }

        private void Update()
        {
            if (dead)
            {
                moveInput = Vector2.zero;
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                var x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                var z = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                moveInput = new Vector2(x, z);
            }
            else
            {
                moveInput = Vector2.zero;
            }

            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || Time.time < nextThrowTime)
            {
                return;
            }

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            if (aimCamera == null)
            {
                return;
            }

            var ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (!AimRules.TryGroundPoint(ray, 0f, out var aimPoint))
            {
                return;
            }

            TryThrowAt(aimPoint);
        }

        private void FixedUpdate()
        {
            if (dead || body == null)
            {
                return;
            }

            body.linearVelocity = PlayerMotion.CalculateVelocity(
                body.linearVelocity,
                moveInput,
                moveSpeed,
                acceleration,
                Time.fixedDeltaTime);
        }

        public bool TryThrowAt(Vector3 worldAimPoint)
        {
            if (dead || orbit == null || Time.time < nextThrowTime)
            {
                return false;
            }

            var direction = worldAimPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < aimDeadZone * aimDeadZone)
            {
                return false;
            }

            var didThrow = orbit.TryThrow(direction.normalized, throwSpeed);
            if (didThrow)
            {
                nextThrowTime = Time.time + throwCooldown;
            }

            return didThrow;
        }

        public void Die()
        {
            if (dead)
            {
                return;
            }

            dead = true;
            moveInput = Vector2.zero;

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            Died?.Invoke();
        }
    }
}
