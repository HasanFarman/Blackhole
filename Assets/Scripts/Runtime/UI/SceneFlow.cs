using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Loads game scenes with the shared transition/state reset.</summary>
    public static class SceneFlow
    {
        public const string MenuScene = "MainMenu";
        public const string GameScene = "Main";

        public static void LoadMenu()
        {
            Time.timeScale = 1f;
            PauseMenu.ResetStatics();
            UnityEngine.SceneManagement.SceneManager.LoadScene(MenuScene);
        }

        public static void LoadGame()
        {
            Time.timeScale = 1f;
            PauseMenu.ResetStatics();
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameScene);
        }
    }
}
