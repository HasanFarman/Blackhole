using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blackhole.Tests
{
    public class EnemyTests
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
        public IEnumerator ContactWithPlayer_KillsPlayerOnce()
        {
            var player = CreatePlayer(out var controller);
            var enemy = CreateEnemy(new Vector3(1.5f, 0f, 0f), player.transform, null);
            var deathCount = 0;
            controller.Died += () => deathCount++;

            yield return new WaitForSeconds(1.5f);

            Assert.IsTrue(controller.IsDead);
            Assert.AreEqual(1, deathCount);
            Assert.IsFalse(enemy.IsDead);
        }

        [UnityTest]
        public IEnumerator Die_ReportsOnceAndSpawnsOneDebris()
        {
            var player = CreatePlayer(out _);
            var field = CreateField(player.transform, out _);
            var enemy = CreateEnemy(new Vector3(3f, 0f, 0f), player.transform, field);
            var diedCount = 0;
            enemy.Died += _ => diedCount++;

            enemy.Die();
            enemy.Die();
            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(1, diedCount);
            Assert.AreEqual(1, field.ActiveCount);
            Assert.IsTrue(enemy == null);
        }

        [UnityTest]
        public IEnumerator ThrownDebris_KillsEnemy()
        {
            var player = CreatePlayer(out _);
            var orbit = player.GetComponent<BlackHoleOrbit>();
            var field = CreateField(player.transform, out var prefab);
            var enemy = CreateEnemy(new Vector3(2f, 0f, 0f), player.transform, field);
            field.Configure(new[] { prefab }, orbit, player.transform, 8);

            var debris = field.SpawnLoose(Vector3.zero, Vector3.zero);
            debris.Launch(Vector3.right, 15f);

            yield return new WaitForSeconds(0.6f);

            Assert.IsTrue(enemy == null || enemy.IsDead);
        }

        private GameObject CreatePlayer(out BlackHoleController controller)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Player";
            player.tag = "Player";
            _created.Add(player);

            var body = player.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;

            var orbit = player.AddComponent<BlackHoleOrbit>();
            orbit.Configure(player.transform, 4f, 2f, 4f, 120f);

            controller = player.AddComponent<BlackHoleController>();
            controller.Configure(orbit, body, null, 7f, 40f, 20f, 0f);
            return player;
        }

        private Enemy CreateEnemy(Vector3 position, Transform target, DebrisField field)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Enemy";
            go.transform.position = position;
            _created.Add(go);

            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;

            var enemy = go.AddComponent<Enemy>();
            enemy.Initialize(target, field);
            return enemy;
        }

        private DebrisField CreateField(Transform player, out GameObject prefab)
        {
            var orbitObject = player.GetComponent<BlackHoleOrbit>();
            var fieldObject = new GameObject("DebrisField");
            _created.Add(fieldObject);
            var field = fieldObject.AddComponent<DebrisField>();

            prefab = new GameObject("DebrisPrefab");
            _created.Add(prefab);
            prefab.SetActive(false);
            var body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;
            var collider = prefab.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            prefab.AddComponent<Debris>();

            field.Configure(new[] { prefab }, orbitObject, player, 8);
            return field;
        }
    }
}
