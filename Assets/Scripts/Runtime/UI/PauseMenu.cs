using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Esc pause card with Resume and Return to main menu.</summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform card;
        [SerializeField] private Image dim;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;

        private bool open;
        private bool transitioning;
        private Coroutine animation;

        public static bool IsPaused { get; private set; }
        public static PauseMenu Instance { get; private set; }

        public void Configure(CanvasGroup canvasGroup, RectTransform cardRect, Image dimImage, Button resume, Button menu)
        {
            group = canvasGroup;
            card = cardRect;
            dim = dimImage;
            resumeButton = resume;
            menuButton = menu;
        }

        private void Awake()
        {
            Instance = this;
            IsPaused = false;
            open = false;

            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
            }

            if (menuButton != null)
            {
                menuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (IsPaused)
            {
                IsPaused = false;
                Time.timeScale = 1f;
            }
        }

        private void Update()
        {
            if (transitioning)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (RunManager.Instance != null && RunManager.Instance.IsGameOver)
            {
                return;
            }

            if (open)
            {
                Resume();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (open || transitioning)
            {
                return;
            }

            open = true;
            IsPaused = true;
            Time.timeScale = 0f;

            if (group != null)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
            }

            if (animation != null)
            {
                StopCoroutine(animation);
            }

            animation = StartCoroutine(Animate(true));
        }

        public void Resume()
        {
            if (!open || transitioning)
            {
                return;
            }

            open = false;
            IsPaused = false;
            Time.timeScale = 1f;

            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            if (animation != null)
            {
                StopCoroutine(animation);
            }

            animation = StartCoroutine(Animate(false));
        }

        public void ReturnToMainMenu()
        {
            if (transitioning)
            {
                return;
            }

            StartCoroutine(ReturnRoutine());
        }

        private IEnumerator ReturnRoutine()
        {
            transitioning = true;
            IsPaused = false;
            open = false;

            var start = dim != null ? dim.color : Color.black;

            for (var elapsed = 0f; elapsed < 0.45f; elapsed += Time.unscaledDeltaTime)
            {
                if (dim != null)
                {
                    dim.color = Color.Lerp(start, new Color(0.012f, 0.02f, 0.047f, 1f), Mathf.Clamp01(elapsed / 0.45f));
                }

                if (card != null)
                {
                    card.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.98f, Mathf.Clamp01(elapsed / 0.45f));
                }

                yield return null;
            }

            SceneFlow.LoadMenu();
        }

        private IEnumerator Animate(bool showing)
        {
            transitioning = true;
            var from = group != null ? group.alpha : 0f;
            var to = showing ? 1f : 0f;
            var cardFrom = card != null ? card.localScale : Vector3.one;
            var cardTo = showing ? Vector3.one : Vector3.one * 0.96f;

            for (var elapsed = 0f; elapsed < 0.22f; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.22f));
                if (group != null)
                {
                    group.alpha = Mathf.Lerp(from, to, t);
                }

                if (card != null)
                {
                    card.localScale = Vector3.Lerp(cardFrom, cardTo, t);
                }

                yield return null;
            }

            if (group != null)
            {
                group.alpha = to;
            }

            if (card != null)
            {
                card.localScale = cardTo;
            }

            transitioning = false;
        }

        public static void ResetStatics()
        {
            IsPaused = false;
        }
    }
}
