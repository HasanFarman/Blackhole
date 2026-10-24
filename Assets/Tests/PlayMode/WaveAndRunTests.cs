using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Blackhole.Tests
{
    public class WaveAndRunTests
    {
        private readonly System.Collections.Generic.List<GameObject> _created = new();

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var go in _created)
            {
                if (go != null)
                {
                    UnityEngine.Object.Destroy(go);
                }
            }

            _created.Clear();
        }

        [UnityTest]
        public IEnumerator GetRandomSpawnPoint_StaysInsideConfiguredRing()
        {
            var player = CreateObject("Player");
            player.transform.position = new Vector3(4f, 1f, -2f);
            var spawner = CreateSpawner();
            spawner.Configure(player.transform, null, null, Array.Empty<GameObject>(), 10f, 12f);

            for (var i = 0; i < 20; i++)
            {
                var point = spawner.GetRandomSpawnPoint();
                var distance = Vector3.Distance(player.transform.position, point);
                Assert.That(distance, Is.InRange(10f, 12f));
                Assert.AreEqual(player.transform.position.y, point.y, 0.001f);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ClearedWave_StartsBreakAndSpawnsBonusDebris()
        {
            var player = CreateObject("Player");
            player.transform.position = new Vector3(0f, 1f, 0f);
            var orbit = player.AddComponent<BlackHoleOrbit>();
            orbit.Configure(player.transform, 4.5f, 2.2f, 3.8f, 120f);

            var field = CreateObject("DebrisField").AddComponent<DebrisField>();
            var prefab = CreateDebrisPrefab();
            field.Configure(new[] { prefab }, orbit, player.transform, 20);

            var spawner = CreateSpawner();
            spawner.Configure(
                player.transform,
                field,
                null,
                Array.Empty<GameObject>(),
                10f,
                12f,
                baseCount: 0,
                addedPerWave: 0,
                breakSeconds: 1f,
                bonusDebris: 3,
                interval: 0.1f);

            spawner.Begin();
            yield return null;
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(spawner.IsBreak);
            Assert.AreEqual(3, field.ActiveCount);
            Assert.That(spawner.BreakSecondsRemaining, Is.GreaterThan(0f));

            var runManager = CreateObject("RunManager").AddComponent<RunManager>();
            runManager.Configure(null, spawner, field, null, 0);
            runManager.GameOver(1);

            Assert.IsFalse(spawner.IsRunning);
            Assert.IsFalse(spawner.IsBreak);
        }

        [Test]
        public void GameOver_StopsSpawnerAndShowsHud()
        {
            var player = CreateObject("Player");
            var spawner = CreateSpawner();
            var runManager = CreateObject("RunManager").AddComponent<RunManager>();
            var hud = CreateObject("HUD").AddComponent<HUD>();

            spawner.Configure(player.transform, null, runManager, Array.Empty<GameObject>());
            runManager.Configure(null, spawner, null, hud, 0);
            spawner.Begin();
            Assert.IsTrue(spawner.IsRunning);

            runManager.GameOver(3);

            Assert.IsTrue(runManager.IsGameOver);
            Assert.IsFalse(spawner.IsRunning);
            Assert.IsTrue(hud.IsGameOverVisible);
        }

        private GameObject CreateObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        private EnemySpawner CreateSpawner()
        {
            return CreateObject("EnemySpawner").AddComponent<EnemySpawner>();
        }

        private GameObject CreateDebrisPrefab()
        {
            var prefab = CreateObject("DebrisPrefab");
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
