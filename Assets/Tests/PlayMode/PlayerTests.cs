using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blackhole.Tests
{
    public class PlayerTests
    {
        private readonly System.Collections.Generic.List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _created)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }

            _created.Clear();
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator TryThrowAt_WithEmptyOrbit_ReturnsFalse()
        {
            var player = CreatePlayer(out var controller, out _);

            Assert.IsFalse(controller.TryThrowAt(new Vector3(5f, 0f, 0f)));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryThrowAt_LaunchesDebrisTowardAimPoint()
        {
            var player = CreatePlayer(out var controller, out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(controller.TryThrowAt(new Vector3(5f, 0f, 0f)));
            Assert.AreEqual(0, orbit.OrbitingCount);
            Assert.AreEqual(DebrisState.Thrown, debris.State);
            Assert.Greater(debris.Body.linearVelocity.x, 0f);
        }

        [UnityTest]
        public IEnumerator TryThrowAt_AtOwnPosition_ReturnsFalse()
        {
            var player = CreatePlayer(out var controller, out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(controller.TryThrowAt(player.transform.position));
            Assert.AreEqual(1, orbit.OrbitingCount);
        }

        [UnityTest]
        public IEnumerator TryThrowAt_NearPlayer_DoesNotThrow()
        {
            var player = CreatePlayer(out var controller, out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(controller.TryThrowAt(new Vector3(1f, 0f, 0f)));
            Assert.AreEqual(1, orbit.OrbitingCount);
            Assert.AreEqual(DebrisState.Orbiting, debris.State);
        }

        [UnityTest]
        public IEnumerator TryThrowAt_RespectsCooldown()
        {
            var player = CreatePlayer(out var controller, out var orbit);
            var body = player.GetComponent<Rigidbody>();
            controller.Configure(orbit, body, null, 7f, 40f, 20f, 1f);

            var first = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            var second = CreateDebris(orbit, player.transform, new Vector3(0f, 0f, 3f));
            orbit.Capture(first);
            orbit.Capture(second);
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(controller.TryThrowAt(new Vector3(5f, 0f, 0f)));
            Assert.IsFalse(controller.TryThrowAt(new Vector3(5f, 0f, 0f)));
            Assert.AreEqual(1, orbit.OrbitingCount);
        }

        [Test]
        public void Die_InvokesDiedOnce()
        {
            var player = CreatePlayer(out var controller, out _);
            var deathCount = 0;
            controller.Died += () => deathCount++;

            controller.Die();
            controller.Die();

            Assert.IsTrue(controller.IsDead);
            Assert.AreEqual(1, deathCount);
        }

        [Test]
        public void CameraFollow_SnapsToConfiguredPitch()
        {
            var cameraObject = new GameObject("Camera");
            _created.Add(cameraObject);
            var target = new GameObject("Target");
            _created.Add(target);
            target.transform.position = new Vector3(5f, 1f, 0f);

            var follow = cameraObject.AddComponent<CameraFollow>();
            follow.Configure(target.transform, 65f, 14f, 8f);
            follow.SnapToTarget();

            Assert.That(cameraObject.transform.eulerAngles.x, Is.InRange(64.5f, 65.5f));
            Assert.Greater(cameraObject.transform.position.y, target.transform.position.y);
        }

        [Test]
        public void CameraFollow_ClampsPitchToPlayableRange()
        {
            var cameraObject = new GameObject("Camera");
            _created.Add(cameraObject);
            var target = new GameObject("Target");
            _created.Add(target);

            var follow = cameraObject.AddComponent<CameraFollow>();
            follow.Configure(target.transform, 90f, 14f, 8f);
            follow.SnapToTarget();

            Assert.That(cameraObject.transform.eulerAngles.x, Is.InRange(59.5f, 70.5f));
        }

        private GameObject CreatePlayer(out BlackHoleController controller, out BlackHoleOrbit orbit)
        {
            var player = new GameObject("Player");
            _created.Add(player);

            var body = player.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;

            var collider = player.AddComponent<SphereCollider>();
            collider.radius = 0.5f;

            orbit = player.AddComponent<BlackHoleOrbit>();
            orbit.Configure(player.transform, 4f, 2f, 4f, 120f);

            controller = player.AddComponent<BlackHoleController>();
            controller.Configure(orbit, body, null, 7f, 40f, 20f, 0f);
            return player;
        }

        private Debris CreateDebris(BlackHoleOrbit orbit, Transform player, Vector3 position)
        {
            var go = new GameObject("Debris");
            _created.Add(go);
            go.layer = 8;
            go.transform.position = position;

            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            var debris = go.AddComponent<Debris>();
            debris.Configure(orbit, player, null);
            return debris;
        }
    }
}
