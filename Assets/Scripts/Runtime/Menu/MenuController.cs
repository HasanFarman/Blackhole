using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Poster main menu: intro reveal, keyboard/mouse navigation, scene loading.</summary>
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private MenuEntry[] entries;
        [SerializeField] private ScreenFade fade;
        [SerializeField] private CanvasGroup howToGroup;
        [SerializeField] private Button howToCloseButton;
        [SerializeField] private MenuCamera menuCamera;

        private Vector2 titleHome;
        private int selection;
        private bool introDone;
        private bool loading;
        private bool howToOpen;

        public void Configure(
            CanvasGroup titleCanvasGroup,
            RectTransform titleTransform,
            MenuEntry[] menuEntries,
            ScreenFade screenFade,
            CanvasGroup howToCanvasGroup,
            Button howToClose,
            MenuCamera camera)
        {
            titleGroup = titleCanvasGroup;
            titleRect = titleTransform;
            entries = menuEntries;
            fade = screenFade;
            howToGroup = howToCanvasGroup;
            howToCloseButton = howToClose;
            menuCamera = camera;
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (titleRect != null)
            {
                titleHome = titleRect.anchoredPosition;
                titleRect.anchoredPosition = titleHome + Vector2.down * 26f;
            }

            if (titleGroup != null)
            {
                titleGroup.alpha = 0f;
            }

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry != null)
                    {
                        entry.SnapReveal(false);
                        entry.Hovered += HandleHover;
                        entry.Clicked += HandleClick;
                    }
                }
            }

            SetHowTo(false);
            Select(0, true);

            if (howToCloseButton != null)
            {
                howToCloseButton.onClick.AddListener(CloseHowTo);
            }

            menuCamera?.BeginIntro(24f, 2.6f);
            StartCoroutine(Intro());
        }

        private void OnDestroy()
        {
            if (entries == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry != null)
                {
                    entry.Hovered -= HandleHover;
                    entry.Clicked -= HandleClick;
                }
            }
        }

        private IEnumerator Intro()
        {
            var startedAt = Time.unscaledTime;

            while (true)
            {
                var elapsed = Time.unscaledTime - startedAt;
                if (elapsed >= 2.1f)
                {
                    break;
                }

                if (fade != null)
                {
                    fade.SetAlpha(1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 1.7f)));
                }

                if (titleGroup != null)
                {
                    titleGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 0.25f) / 1.05f));
                }

                if (titleRect != null)
                {
                    var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 0.25f) / 1.3f));
                    titleRect.anchoredPosition = Vector2.Lerp(titleHome + Vector2.down * 26f, titleHome, t);
                }

                if (entries != null)
                {
                    for (var i = 0; i < entries.Length; i++)
                    {
                        if (entries[i] != null)
                        {
                            entries[i].Reveal(elapsed >= 0.55f + i * 0.12f);
                        }
                    }
                }

                yield return null;
            }

            if (fade != null)
            {
                fade.SetAlpha(0f);
            }

            if (titleGroup != null)
            {
                titleGroup.alpha = 1f;
            }

            if (titleRect != null)
            {
                titleRect.anchoredPosition = titleHome;
            }

            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    entry?.SnapReveal(true);
                }
            }

            introDone = true;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (howToOpen)
                {
                    CloseHowTo();
                }

                return;
            }

            if (!introDone || loading || howToOpen)
            {
                return;
            }

            var up = keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame;
            var down = keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame;
            if (up)
            {
                Select(selection - 1);
            }
            else if (down)
            {
                Select(selection + 1);
            }

            var confirm = keyboard.enterKey.wasPressedThisFrame ||
                          keyboard.numpadEnterKey.wasPressedThisFrame ||
                          keyboard.spaceKey.wasPressedThisFrame;

            if (confirm && entries != null && selection >= 0 && selection < entries.Length)
            {
                Activate(entries[selection]);
            }
        }

        private void Select(int index, bool instant = false)
        {
            if (entries == null || entries.Length == 0)
            {
                return;
            }

            selection = (index % entries.Length + entries.Length) % entries.Length;

            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i] == null)
                {
                    continue;
                }

                entries[i].SetSelected(i == selection);
                if (instant)
                {
                    entries[i].SetSelected(i == selection);
                }
            }
        }

        private void HandleHover(MenuEntry entry)
        {
            if (!introDone || loading || howToOpen || entry == null)
            {
                return;
            }

            var index = System.Array.IndexOf(entries, entry);
            if (index >= 0)
            {
                Select(index);
            }
        }

        private void HandleClick(MenuEntry entry)
        {
            if (!introDone || loading || howToOpen)
            {
                return;
            }

            Activate(entry);
        }

        private void Activate(MenuEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            switch (entry.Action)
            {
                case MenuAction.Play:
                    Play();
                    break;
                case MenuAction.HowToPlay:
                    OpenHowTo();
                    break;
                case MenuAction.Quit:
                    Quit();
                    break;
            }
        }

        public void Play()
        {
            if (loading)
            {
                return;
            }

            loading = true;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            if (fade != null)
            {
                yield return fade.FadeTo(1f, 0.55f);
            }

            SceneFlow.LoadGame();
        }

        public void OpenHowTo()
        {
            SetHowTo(true);
        }

        public void CloseHowTo()
        {
            SetHowTo(false);
        }

        private void SetHowTo(bool open)
        {
            howToOpen = open;

            if (howToGroup == null)
            {
                return;
            }

            howToGroup.alpha = open ? 1f : 0f;
            howToGroup.blocksRaycasts = open;
            howToGroup.interactable = open;

            if (open)
            {
                Select(1);
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
