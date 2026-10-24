using UnityEngine;

namespace Blackhole
{
    public static class WaveRules
    {
        public static int EnemyCount(int wave, int baseEnemyCount, int enemiesAddedPerWave)
        {
            var clampedWave = Mathf.Max(1, wave);
            var count = baseEnemyCount + (clampedWave - 1) * enemiesAddedPerWave;
            return Mathf.Max(0, count);
        }
    }
}
