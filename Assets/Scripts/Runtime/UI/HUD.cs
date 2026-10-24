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
        [SerializeField] private CanvasGroup gameOverGroup;
        [SerializeField] private Button menuButton;

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
            Text gameOverLabel,
            CanvasGroup gameOverCanvasGroup = null,
            Button returnToMenuButton = null)
        {
            spawner = enemySpawner;
            orbit = blackHoleOrbit;
            waveText = wave;
            enemyText = enemies;
            debrisText = debris;
            breakText = breakLabel;
            gameOverPanel = gameOver;
            gameOverText = gameOverLabel;
            gameOverGroup = gameOverCanvasGroup;
            menuButton = returnToMenuButton;
        }

        private void Awake()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ReturnToMenu);
            }
        }

        private void OnDestroy()
        {
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(ReturnToMenu);
            }
        }

        private void Update()
        {
            if (spawner != null)
            {
                if (waveText != null)
                {
                    waveText.text = spawner.CurrentWave.ToString("00");
                }

                if (enemyText != null)
                {
                    enemyText.text = spawner.AliveEnemies.ToString("00");
                }

                if (breakText != null)
                {
                    breakText.gameObject.SetActive(spawner.IsBreak);
                    if (spawner.IsBreak)
                    {
                        breakText.text = "NEXT WAVE IN " + Mathf.CeilToInt(spawner.BreakSecondsRemaining);
                    }
                }
            }

            if (orbit != null && debrisText != null)
            {
                debrisText.text = orbit.OrbitingCount.ToString("00");
            }
        }

        public void ShowGameOver(int wave)
        {
            gameOverVisible = true;

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverGroup != null)
            {
                gameOverGroup.alpha = 1f;
                gameOverGroup.blocksRaycasts = true;
                gameOverGroup.interactable = true;
            }

            if (gameOverText != null)
            {
                gameOverText.text = "REACHED WAVE " + wave.ToString("00");
            }
        }

        public void ReturnToMenu()
        {
            SceneFlow.LoadMenu();
        }
    }
}
