using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blackhole
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private DebrisField debrisField;
        [SerializeField] private RunManager runManager;
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private int baseEnemyCount = 5;
        [SerializeField] private int enemiesAddedPerWave = 3;
        [SerializeField] private float spawnInterval = 1.2f;
        [SerializeField] private float spawnRadiusMin = 13f;
        [SerializeField] private float spawnRadiusMax = 17f;
        [SerializeField] private float waveBreakSeconds = 5f;
        [SerializeField] private int bonusDebrisCount = 3;

        private readonly HashSet<Enemy> alive = new();

        public int CurrentWave { get; private set; }
        public int AliveEnemies { get; private set; }
        public int TotalEnemiesThisWave { get; private set; }
        public bool IsBreak { get; private set; }
        public float BreakSecondsRemaining { get; private set; }
        public bool IsRunning { get; private set; }

        public void Configure(
            Transform playerTransform,
            DebrisField field,
            RunManager manager,
            GameObject[] prefabs,
            float minRadius = 13f,
            float maxRadius = 17f)
        {
            player = playerTransform;
            debrisField = field;
            runManager = manager;
            enemyPrefabs = prefabs;
            spawnRadiusMin = minRadius;
            spawnRadiusMax = maxRadius;
        }

        public void Begin()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            StartCoroutine(RunWaves());
        }

        public void Stop()
        {
            IsRunning = false;
            StopAllCoroutines();
            IsBreak = false;
            BreakSecondsRemaining = 0f;
        }

        public Vector3 GetRandomSpawnPoint()
        {
            var center = player != null ? player.position : transform.position;
            var angle = Random.Range(0f, 360f);
            var radius = Random.Range(spawnRadiusMin, spawnRadiusMax);
            return OrbitMath.OrbitPosition(center, radius, angle);
        }

        private IEnumerator RunWaves()
        {
            while (ShouldContinue())
            {
                CurrentWave++;
                TotalEnemiesThisWave = WaveRules.EnemyCount(CurrentWave, baseEnemyCount, enemiesAddedPerWave);
                alive.Clear();
                AliveEnemies = 0;
                IsBreak = false;
                BreakSecondsRemaining = 0f;

                for (var i = 0; i < TotalEnemiesThisWave; i++)
                {
                    if (!ShouldContinue())
                    {
                        yield break;
                    }

                    SpawnEnemy();
                    if (i < TotalEnemiesThisWave - 1)
                    {
                        yield return new WaitForSeconds(spawnInterval);
                    }
                }

                while (alive.Count > 0)
                {
                    if (!ShouldContinue())
                    {
                        yield break;
                    }

                    yield return null;
                }

                if (!ShouldContinue())
                {
                    yield break;
                }

                IsBreak = true;
                debrisField?.SpawnBonusNearPlayer(bonusDebrisCount);

                for (var timer = waveBreakSeconds; timer > 0f; timer -= 1f)
                {
                    if (!ShouldContinue())
                    {
                        yield break;
                    }

                    BreakSecondsRemaining = timer;
                    yield return new WaitForSeconds(1f);
                }

                BreakSecondsRemaining = 0f;
                IsBreak = false;
            }

            IsRunning = false;
        }

        private bool ShouldContinue()
        {
            return IsRunning && (runManager == null || !runManager.IsGameOver);
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                return;
            }

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (prefab == null)
            {
                return;
            }

            var instance = Instantiate(prefab, GetRandomSpawnPoint(), Random.rotation);
            var enemy = instance.GetComponent<Enemy>();
            if (enemy == null)
            {
                Destroy(instance);
                return;
            }

            enemy.Initialize(player, debrisField);
            enemy.Died += OnEnemyDied;
            alive.Add(enemy);
            AliveEnemies = alive.Count;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.Died -= OnEnemyDied;
            alive.Remove(enemy);
            AliveEnemies = alive.Count;
        }
    }
}
