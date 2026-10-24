using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Blackhole.Tests
{
    public class MainSceneSmokeTests
    {
        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator MainScene_StartsWithOrbitCameraAndWave()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<BlackHoleController>();
            var orbit = Object.FindFirstObjectByType<BlackHoleOrbit>();
            var camera = Object.FindFirstObjectByType<CameraFollow>();
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            var hud = Object.FindFirstObjectByType<HUD>();

            Assert.IsNotNull(player);
            Assert.IsNotNull(orbit);
            Assert.AreEqual(6, orbit.OrbitingCount);
            Assert.IsNotNull(camera);
            Assert.That(camera.transform.eulerAngles.x, Is.InRange(60f, 70f));
            Assert.IsNotNull(spawner);
            Assert.IsNotNull(hud);

            yield return new WaitForSeconds(1f);
            Assert.GreaterOrEqual(spawner.AliveEnemies, 1);
        }

        [UnityTest]
        public IEnumerator PlayerDeath_ShowsGameOver()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<BlackHoleController>();
            var runManager = Object.FindFirstObjectByType<RunManager>();
            var hud = Object.FindFirstObjectByType<HUD>();

            Assert.IsNotNull(player);
            Assert.IsNotNull(runManager);
            Assert.IsNotNull(hud);

            player.Die();
            yield return null;

            Assert.IsTrue(runManager.IsGameOver);
            Assert.IsTrue(hud.IsGameOverVisible);
        }

        [UnityTest]
        public IEnumerator RestartButton_ReloadsRun()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<BlackHoleController>();
            var runManager = Object.FindFirstObjectByType<RunManager>();

            player.Die();
            yield return null;
            yield return null;

            var restartButton = Object.FindFirstObjectByType<RestartButton>();
            Assert.IsTrue(runManager.IsGameOver);
            Assert.IsNotNull(restartButton);

            restartButton.GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;

            var reloadedRun = Object.FindFirstObjectByType<RunManager>();
            var reloadedPlayer = Object.FindFirstObjectByType<BlackHoleController>();
            Assert.IsNotNull(reloadedRun);
            Assert.IsFalse(reloadedRun.IsGameOver);
            Assert.IsFalse(reloadedPlayer.IsDead);
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
