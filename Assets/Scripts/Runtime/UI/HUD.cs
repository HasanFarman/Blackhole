using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private BlackHoleOrbit orbit;
        [SerializeField] private Text waveText;
        [SerializeField] private Text enemyText;
        [SerializeField] private Text debrisText;
        [SerializeField] private Text breakText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;

        private bool gameOverVisible;

        public bool IsGameOverVisible => gameOverVisible;

        public void Configure(
            EnemySpawner enemySpawner,
            BlackHoleOrbit blackHoleOrbit,
            Text wave,
            Text enemies,
            Text debris,
            Text breakLabel,
            GameObject gameOver,
            Text gameOverLabel)
        {
            spawner = enemySpawner;
            orbit = blackHoleOrbit;
            waveText = wave;
            enemyText = enemies;
            debrisText = debris;
            breakText = breakLabel;
            gameOverPanel = gameOver;
            gameOverText = gameOverLabel;
        }

        private void Awake()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (spawner != null)
            {
                if (waveText != null)
                {
                    waveText.text = $"Wave {spawner.CurrentWave}";
                }

                if (enemyText != null)
                {
                    enemyText.text = $"Enemies {spawner.AliveEnemies}";
                }

                if (breakText != null)
                {
                    breakText.gameObject.SetActive(spawner.IsBreak);
                    if (spawner.IsBreak)
                    {
                        breakText.text = $"Next wave in {Mathf.CeilToInt(spawner.BreakSecondsRemaining)}";
                    }
                }
            }

            if (orbit != null && debrisText != null)
            {
                debrisText.text = $"Debris {orbit.OrbitingCount}";
            }
        }

        public void ShowGameOver(int wave)
        {
            gameOverVisible = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverText != null)
            {
                gameOverText.text = $"Run over\nReached wave {wave}\nPress R or click Restart";
            }
        }
    }
}
