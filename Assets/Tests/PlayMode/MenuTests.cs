using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Blackhole.Tests
{
    public class MenuTests
    {
        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator MainMenu_IntroRevealsMenuAndHole()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;
            yield return null;

            Assert.IsNotNull(Object.FindFirstObjectByType<MenuCamera>());
            Assert.IsNotNull(Object.FindFirstObjectByType<MenuBlackHole>());
            Assert.IsNotNull(Object.FindFirstObjectByType<MenuController>());

            yield return new WaitForSeconds(3f);

            var playEntry = GameObject.Find("/MenuCanvas/Menu/PlayEntry");
            Assert.IsNotNull(playEntry);

            var group = playEntry.GetComponent<CanvasGroup>();
            Assert.IsNotNull(group);
            Assert.Greater(group.alpha, 0.9f);
        }

        [UnityTest]
        public IEnumerator PauseMenu_PausesAndResumes()
        {
            SceneManager.LoadScene("Main");
            yield return null;
            yield return null;

            var pause = Object.FindFirstObjectByType<PauseMenu>();
            Assert.IsNotNull(pause);

            pause.Open();
            Assert.IsTrue(PauseMenu.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);

            yield return new WaitForSecondsRealtime(0.3f);

            pause.Resume();
            Assert.IsFalse(PauseMenu.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
