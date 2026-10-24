using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Blackhole
{
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private BlackHoleController player;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private DebrisField debrisField;
        [SerializeField] private HUD hud;
        [SerializeField] private int initialDebrisCount = 6;

        public static RunManager Instance { get; private set; }
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Configure(
            BlackHoleController playerController,
            EnemySpawner enemySpawner,
            DebrisField field,
            HUD hudDisplay,
            int startingDebris = 6)
        {
            player = playerController;
            spawner = enemySpawner;
            debrisField = field;
            hud = hudDisplay;
            initialDebrisCount = startingDebris;
        }

        private void Start()
        {
            if (IsGameOver)
            {
                return;
            }

            if (player != null)
            {
                player.Died += HandlePlayerDied;
            }

            debrisField?.SpawnInitialOrbit(initialDebrisCount);
            spawner?.Begin();
        }

        private void Update()
        {
            if (!IsGameOver)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                Restart();
            }
        }

        private void HandlePlayerDied()
        {
            GameOver(spawner != null ? spawner.CurrentWave : 0);
        }

        public void GameOver(int wave)
        {
            if (IsGameOver)
            {
                return;
            }

            IsGameOver = true;
            spawner?.Stop();
            hud?.ShowGameOver(wave);
            Time.timeScale = 0f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.Died -= HandlePlayerDied;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
