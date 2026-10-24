using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackhole
{
    public enum MenuAction
    {
        Play,
        HowToPlay,
        Quit
    }

    /// <summary>A quiet poster-style menu row: number, label, hairline rule.</summary>
    public class MenuEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private Text indexLabel;
        [SerializeField] private Image rule;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private MenuAction action;

        private RectTransform rect;
        private Vector2 basePosition;
        private float selected;
        private float selectedTarget;
        private float reveal;
        private float revealTarget;
        private float revealDelay;

        public MenuAction Action => action;
        public event Action<MenuEntry> Hovered;
        public event Action<MenuEntry> Clicked;

        public void Configure(Text labelText, Text numberLabel, Image ruleImage, CanvasGroup canvasGroup, MenuAction menuAction)
        {
            label = labelText;
            indexLabel = numberLabel;
            rule = ruleImage;
            group = canvasGroup;
            action = menuAction;
        }

        private void Awake()
        {
            Capture();
            Apply();
        }

        private void Capture()
        {
            rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                basePosition = rect.anchoredPosition;
            }

            if (group == null)
            {
                group = GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Update()
        {
            var step = Time.unscaledDeltaTime;

            if (revealDelay > 0f)
            {
                revealDelay -= step;
            }
            else
            {
                reveal = Mathf.MoveTowards(reveal, revealTarget, step * 2.6f);
            }

            selected = Mathf.MoveTowards(selected, selectedTarget, step * 10f);
            Apply();
        }

        public void SetSelected(bool isSelected)
        {
            selectedTarget = isSelected ? 1f : 0f;
        }

        public void Reveal(bool show, float delay = 0f)
        {
            revealTarget = show ? 1f : 0f;
            revealDelay = delay;
        }

        public void SnapReveal(bool show)
        {
            revealTarget = show ? 1f : 0f;
            reveal = revealTarget;
            revealDelay = 0f;
            selected = selectedTarget;
            Apply();
        }

        private void Apply()
        {
            if (rect != null)
            {
                rect.anchoredPosition = basePosition + Vector2.right * (14f * selected);
            }

            if (label != null)
            {
                label.color = Color.Lerp(UITheme.Muted, UITheme.Text, selected);
            }

            if (indexLabel != null)
            {
                indexLabel.color = Color.Lerp(UITheme.Faint, UITheme.Cyan, selected);
            }

            if (rule != null)
            {
                rule.color = Color.Lerp(
                    UITheme.Faint,
                    new Color(UITheme.Cyan.r, UITheme.Cyan.g, UITheme.Cyan.b, 0.75f),
                    selected);
            }

            if (group != null)
            {
                group.alpha = reveal;
                group.blocksRaycasts = reveal > 0.9f;
                group.interactable = reveal > 0.9f;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hovered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }
    }
}
