using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blackhole.Tests
{
    public class DebrisTests
    {
        private const int DebrisLayer = 8;

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
        public IEnumerator Capture_AddsDebrisToOrbit()
        {
            var player = CreatePlayer(out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));

            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(1, orbit.OrbitingCount);
            Assert.AreEqual(DebrisState.Orbiting, debris.State);
            Assert.IsTrue(debris.Body.isKinematic);
        }

        [UnityTest]
        public IEnumerator TryThrow_WithEmptyOrbit_ReturnsFalse()
        {
            CreatePlayer(out var orbit);

            Assert.IsFalse(orbit.TryThrow(Vector3.right, 12f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryThrow_LaunchesBestAlignedPiece()
        {
            var player = CreatePlayer(out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(orbit.TryThrow(Vector3.right, 12f));
            Assert.AreEqual(0, orbit.OrbitingCount);
            Assert.AreEqual(DebrisState.Thrown, debris.State);
            Assert.Greater(debris.Body.linearVelocity.x, 0f);
        }

        [UnityTest]
        public IEnumerator GravityWell_PullsLooseDebrisTowardCenter()
        {
            var player = CreatePlayer(out var orbit);
            orbit.Configure(player.transform, 0.5f, 0.1f, 0.2f, 120f);
            var well = player.AddComponent<GravityWell>();
            well.Configure(10f, 20f, 1 << DebrisLayer);
            var debris = CreateDebris(orbit, player.transform, new Vector3(5f, 0f, 0f));

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.Less(debris.Body.linearVelocity.x, 0f);
            Assert.AreEqual(DebrisState.Loose, debris.State);
        }

        [UnityTest]
        public IEnumerator DebrisField_AtCap_DestroysOldestPiece()
        {
            var player = CreatePlayer(out var orbit);
            var fieldObject = new GameObject("DebrisField");
            _created.Add(fieldObject);
            var field = fieldObject.AddComponent<DebrisField>();
            var prefab = CreateDebrisPrefab();
            field.Configure(new[] { prefab }, orbit, player.transform, 2);

            field.SpawnLoose(Vector3.zero, Vector3.zero);
            field.SpawnLoose(Vector3.zero, Vector3.zero);
            field.SpawnLoose(Vector3.zero, Vector3.zero);
            yield return null;

            Assert.AreEqual(2, field.ActiveCount);
        }

        [UnityTest]
        public IEnumerator DestroyingOrbitingDebris_RemovesItFromOrbit()
        {
            var player = CreatePlayer(out var orbit);
            var debris = CreateDebris(orbit, player.transform, new Vector3(3f, 0f, 0f));
            orbit.Capture(debris);
            yield return new WaitForFixedUpdate();

            Object.Destroy(debris.gameObject);
            yield return null;

            Assert.AreEqual(0, orbit.OrbitingCount);
        }

        private GameObject CreatePlayer(out BlackHoleOrbit orbit)
        {
            var player = new GameObject("Player");
            _created.Add(player);
            orbit = player.AddComponent<BlackHoleOrbit>();
            orbit.Configure(player.transform, 4f, 2f, 4f, 120f);
            return player;
        }

        private Debris CreateDebris(BlackHoleOrbit orbit, Transform player, Vector3 position)
        {
            var go = new GameObject("Debris");
            _created.Add(go);
            go.layer = DebrisLayer;
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

        private GameObject CreateDebrisPrefab()
        {
            var prefab = new GameObject("DebrisPrefab");
            _created.Add(prefab);
            prefab.layer = DebrisLayer;
            prefab.SetActive(false);

            var body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;

            var collider = prefab.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            prefab.AddComponent<Debris>();
            return prefab;
        }
    }
}
