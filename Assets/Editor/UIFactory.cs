using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackhole.Editor
{
    public enum TextWeight
    {
        Regular,
        Medium,
        Bold
    }

    /// <summary>Small authoring helpers so every generated screen shares one visual language.</summary>
    public static class UIFactory
    {
        public const string FontFolder = "Assets/Resources/Fonts";

        public static Font Font(TextWeight weight)
        {
            var file = weight switch
            {
                TextWeight.Bold => "SpaceGrotesk-Bold",
                TextWeight.Medium => "SpaceGrotesk-Medium",
                _ => "SpaceGrotesk-Regular"
            };

            var loaded = AssetDatabase.LoadAssetAtPath<Font>($"{FontFolder}/{file}.ttf");
            return loaded != null ? loaded : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var events = Object.FindFirstObjectByType<EventSystem>();
            if (events == null)
            {
                var eventsObject = new GameObject("EventSystem");
                eventsObject.AddComponent<EventSystem>();
                eventsObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            return canvas;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string content,
            int size,
            TextWeight weight,
            TextAnchor alignment,
            Color color,
            bool tracked = false)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var text = textObject.GetComponent<Text>();
            text.font = Font(weight);
            text.fontStyle = FontStyle.Normal;
            text.text = tracked ? UITheme.Track(content) : content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        public static Image CreatePanel(
            Transform parent,
            string name,
            CozyPanelStyle style,
            Color tint,
            bool blockRaycasts = false)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            var image = panelObject.GetComponent<Image>();
            image.color = tint;
            image.raycastTarget = blockRaycasts;
            panelObject.AddComponent<CozyPanel>().Configure(style, tint, blockRaycasts);

            return image;
        }

        public static Button CreatePillButton(Transform parent, string name, string labelText, bool primary)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.62f, 0.87f, 1f, 1f) : new Color(1f, 1f, 1f, 0.06f);
            image.raycastTarget = true;

            var button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            var label = CreateText(
                buttonObject.transform,
                "Label",
                labelText,
                22,
                TextWeight.Medium,
                TextAnchor.MiddleCenter,
                primary ? new Color(0.02f, 0.07f, 0.12f, 1f) : UITheme.Text,
                tracked: true);

            Stretch(label.rectTransform, 24f, 24f, 0f, 0f);

            buttonObject.AddComponent<CozyButton>().Configure(label, primary);
            return button;
        }

        public static MenuEntry CreateMenuEntry(
            Transform parent,
            string name,
            string labelText,
            string indexText,
            MenuAction action)
        {
            var rowObject = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            rowObject.transform.SetParent(parent, false);

            var rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(460f, 66f);

            var group = rowObject.GetComponent<CanvasGroup>();

            var hit = CreatePanel(rowObject.transform, "Hit", CozyPanelStyle.Flat, new Color(0f, 0f, 0f, 0f), true);
            Stretch(hit.rectTransform, 0f, 0f, 0f, 0f);

            var rule = CreatePanel(rowObject.transform, "Rule", CozyPanelStyle.Flat, UITheme.Faint);
            var ruleRect = rule.rectTransform;
            ruleRect.anchorMin = new Vector2(0f, 1f);
            ruleRect.anchorMax = new Vector2(1f, 1f);
            ruleRect.pivot = new Vector2(0.5f, 1f);
            ruleRect.anchoredPosition = Vector2.zero;
            ruleRect.sizeDelta = new Vector2(0f, 1f);

            var label = CreateText(
                rowObject.transform,
                "Label",
                labelText,
                31,
                TextWeight.Medium,
                TextAnchor.MiddleLeft,
                UITheme.Muted,
                tracked: true);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(0f, 1f);
            label.rectTransform.pivot = new Vector2(0f, 0.5f);
            label.rectTransform.anchoredPosition = new Vector2(0f, -2f);
            label.rectTransform.sizeDelta = new Vector2(360f, 0f);

            var indexLabel = CreateText(
                rowObject.transform,
                "Index",
                indexText,
                15,
                TextWeight.Regular,
                TextAnchor.MiddleRight,
                UITheme.Faint);
            indexLabel.rectTransform.anchorMin = new Vector2(1f, 0f);
            indexLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
            indexLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
            indexLabel.rectTransform.anchoredPosition = new Vector2(0f, -2f);
            indexLabel.rectTransform.sizeDelta = new Vector2(80f, 0f);

            var entry = rowObject.AddComponent<MenuEntry>();
            entry.Configure(label, indexLabel, rule, group, action);
            return entry;
        }

        public static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        public static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void TopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            SetRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        }

        public static void TopRight(RectTransform rect, Vector2 position, Vector2 size)
        {
            SetRect(rect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), position, size);
        }

        public static void Centered(RectTransform rect, Vector2 position, Vector2 size)
        {
            SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        }
    }
}
