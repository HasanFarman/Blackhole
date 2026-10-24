using UnityEngine;

namespace Blackhole
{
    /// <summary>Spawns one-shot particle bursts from Resources.</summary>
    public static class FxBurst
    {
        private const string EnemyBurstPath = "FX/EnemyBurst";

        private static GameObject enemyBurstPrefab;

        public static void SpawnEnemy(Vector3 position)
        {
            if (enemyBurstPrefab == null)
            {
                enemyBurstPrefab = Resources.Load<GameObject>(EnemyBurstPath);
            }

            if (enemyBurstPrefab == null)
            {
                return;
            }

            var instance = Object.Instantiate(enemyBurstPrefab, position, Quaternion.identity);
            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>())
            {
                system.Play();
            }
        }
    }
}
