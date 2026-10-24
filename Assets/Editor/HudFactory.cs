using UnityEngine;
using UnityEngine.UI;

namespace Blackhole.Editor
{
    public static partial class ProjectBootstrap
    {
        private static HUD BuildHud(EnemySpawner spawner, BlackHoleOrbit orbit)
        {
            var canvas = UIFactory.CreateCanvas("HUD", 10);
            var root = canvas.transform;

            // ---- wave / enemies card -------------------------------------------
            var waveCard = UIFactory.CreatePanel(root, "WaveCard", CozyPanelStyle.Card, UITheme.Panel);
            UIFactory.TopLeft(waveCard.rectTransform, new Vector2(32f, -28f), new Vector2(368f, 124f));

            var waveLabel = UIFactory.CreateText(
                waveCard.transform, "WaveLabel", "WAVE", 15, TextWeight.Regular,
                TextAnchor.UpperLeft, UITheme.Muted, tracked: true);
            UIFactory.TopLeft(waveLabel.rectTransform, new Vector2(26f, -18f), new Vector2(200f, 22f));

            var waveText = UIFactory.CreateText(
                waveCard.transform, "WaveText", "01", 58, TextWeight.Bold,
                TextAnchor.UpperLeft, UITheme.Text);
            UIFactory.TopLeft(waveText.rectTransform, new Vector2(22f, -34f), new Vector2(150f, 76f));

            var divider = UIFactory.CreatePanel(waveCard.transform, "Divider", CozyPanelStyle.Flat, UITheme.Faint);
            UIFactory.SetRect(
                divider.rectTransform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(196f, -30f), new Vector2(1f, 64f));

            var enemiesLabel = UIFactory.CreateText(
                waveCard.transform, "EnemiesLabel", "ENEMIES", 13, TextWeight.Regular,
                TextAnchor.UpperLeft, UITheme.Muted, tracked: true);
            UIFactory.TopLeft(enemiesLabel.rectTransform, new Vector2(222f, -30f), new Vector2(140f, 20f));

            var enemyText = UIFactory.CreateText(
                waveCard.transform, "EnemyText", "00", 34, TextWeight.Medium,
                TextAnchor.UpperLeft, UITheme.Text);
            UIFactory.TopLeft(enemyText.rectTransform, new Vector2(220f, -50f), new Vector2(120f, 50f));

            // ---- debris card ----------------------------------------------------
            var debrisCard = UIFactory.CreatePanel(root, "DebrisCard", CozyPanelStyle.Card, UITheme.Panel);
            UIFactory.TopRight(debrisCard.rectTransform, new Vector2(-32f, -28f), new Vector2(210f, 96f));

            var debrisLabel = UIFactory.CreateText(
                debrisCard.transform, "DebrisLabel", "DEBRIS", 13, TextWeight.Regular,
                TextAnchor.UpperLeft, UITheme.Muted, tracked: true);
            UIFactory.TopLeft(debrisLabel.rectTransform, new Vector2(22f, -18f), new Vector2(120f, 20f));

            var debrisText = UIFactory.CreateText(
                debrisCard.transform, "DebrisText", "00", 36, TextWeight.Medium,
                TextAnchor.UpperLeft, UITheme.Text);
            UIFactory.TopLeft(debrisText.rectTransform, new Vector2(20f, -38f), new Vector2(110f, 50f));

            var debrisRing = UIFactory.CreatePanel(debrisCard.transform, "OrbitIcon", CozyPanelStyle.Ring, UITheme.WithAlpha(UITheme.Cyan, 0.45f));
            UIFactory.SetRect(
                debrisRing.rectTransform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-18f, 0f), new Vector2(58f, 58f));

            // ---- wave break -----------------------------------------------------
            var breakGlow = UIFactory.CreatePanel(root, "BreakGlow", CozyPanelStyle.Glow, UITheme.WithAlpha(UITheme.Cyan, 0.12f));
            UIFactory.Centered(breakGlow.rectTransform, new Vector2(0f, 148f), new Vector2(860f, 240f));

            var breakText = UIFactory.CreateText(
                root, "BreakText", "NEXT WAVE IN 5", 42, TextWeight.Medium,
                TextAnchor.MiddleCenter, UITheme.Text);
            UIFactory.Centered(breakText.rectTransform, new Vector2(0f, 140f), new Vector2(900f, 80f));
            breakText.gameObject.SetActive(false);

            // ---- control hint ---------------------------------------------------
            var controls = UIFactory.CreateText(
                root, "ControlsText",
                "WASD MOVE   ·   MOUSE AIM   ·   LEFT CLICK THROW   ·   ESC PAUSE",
                16, TextWeight.Regular, TextAnchor.MiddleCenter, UITheme.WithAlpha(UITheme.Muted, 0.9f),
                tracked: true);
            UIFactory.SetRect(
                controls.rectTransform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(1200f, 34f));

            // ---- pause ----------------------------------------------------------
            BuildPauseMenu(out _, out _, out _, out _, out _);

            // ---- game over ------------------------------------------------------
            BuildGameOver(out var gameOverRoot, out var gameOverText, out var gameOverGroup, out var returnButton);

            var hud = canvas.gameObject.AddComponent<HUD>();
            hud.Configure(
                spawner,
                orbit,
                waveText,
                enemyText,
                debrisText,
                breakText,
                gameOverRoot,
                gameOverText,
                gameOverGroup,
                returnButton);

            return hud;
        }

        private static void BuildPauseMenu(
            out CanvasGroup group,
            out RectTransform card,
            out Image dim,
            out Button resume,
            out Button menu)
        {
            var canvas = UIFactory.CreateCanvas("PauseCanvas", 20);
            var root = new GameObject("PauseRoot", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(canvas.transform, false);
            var rootRect = root.GetComponent<RectTransform>();
            UIFactory.Stretch(rootRect, 0f, 0f, 0f, 0f);

            group = root.GetComponent<CanvasGroup>();

            dim = UIFactory.CreatePanel(root.transform, "Dim", CozyPanelStyle.Flat, new Color(0.012f, 0.02f, 0.047f, 0.82f), true);
            UIFactory.Stretch(dim.rectTransform, 0f, 0f, 0f, 0f);

            var halo = UIFactory.CreatePanel(root.transform, "Halo", CozyPanelStyle.Ring, UITheme.WithAlpha(UITheme.Cyan, 0.045f));
            UIFactory.Centered(halo.rectTransform, Vector2.zero, new Vector2(1180f, 1180f));
            halo.raycastTarget = false;

            var cardImage = UIFactory.CreatePanel(root.transform, "PauseCard", CozyPanelStyle.Card, UITheme.Panel);
            card = cardImage.rectTransform;
            UIFactory.Centered(card, Vector2.zero, new Vector2(640f, 500f));

            var title = UIFactory.CreateText(
                card, "Title", "PAUSED", 62, TextWeight.Bold,
                TextAnchor.MiddleCenter, UITheme.Text, tracked: true);
            UIFactory.Centered(title.rectTransform, new Vector2(0f, 152f), new Vector2(560f, 90f));
            title.gameObject.AddComponent<UIGradient>().SetColors(UITheme.Text, UITheme.WithAlpha(UITheme.Muted, 0.75f));

            var subtitle = UIFactory.CreateText(
                card, "Subtitle", "TAKE A BREATH", 15, TextWeight.Regular,
                TextAnchor.MiddleCenter, UITheme.Muted, tracked: true);
            UIFactory.Centered(subtitle.rectTransform, new Vector2(0f, 100f), new Vector2(500f, 26f));

            resume = UIFactory.CreatePillButton(card, "ResumeButton", "RESUME", true);
            UIFactory.Centered(resume.GetComponent<RectTransform>(), new Vector2(0f, 6f), new Vector2(360f, 68f));

            menu = UIFactory.CreatePillButton(card, "MainMenuButton", "RETURN TO MAIN MENU", false);
            UIFactory.Centered(menu.GetComponent<RectTransform>(), new Vector2(0f, -78f), new Vector2(360f, 68f));

            var hint = UIFactory.CreateText(
                card, "Hint", "ESC TO RESUME", 14, TextWeight.Regular,
                TextAnchor.MiddleCenter, UITheme.Faint, tracked: true);
            UIFactory.Centered(hint.rectTransform, new Vector2(0f, -156f), new Vector2(400f, 22f));

            var pause = root.AddComponent<PauseMenu>();
            pause.Configure(group, card, dim, resume, menu);
            root.SetActive(true);
        }

        private static void BuildGameOver(
            out GameObject overlay,
            out Text reachedText,
            out CanvasGroup group,
            out Button returnButton)
        {
            var canvas = UIFactory.CreateCanvas("GameOverCanvas", 15);
            overlay = canvas.gameObject;

            var root = new GameObject("GameOverRoot", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(canvas.transform, false);
            UIFactory.Stretch(root.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            group = root.GetComponent<CanvasGroup>();

            var dim = UIFactory.CreatePanel(root.transform, "Dim", CozyPanelStyle.Flat, new Color(0.012f, 0.02f, 0.047f, 0.84f), true);
            UIFactory.Stretch(dim.rectTransform, 0f, 0f, 0f, 0f);

            var halo = UIFactory.CreatePanel(root.transform, "Halo", CozyPanelStyle.Ring, UITheme.WithAlpha(UITheme.Amber, 0.045f));
            UIFactory.Centered(halo.rectTransform, Vector2.zero, new Vector2(1240f, 1240f));
            halo.raycastTarget = false;

            var card = UIFactory.CreatePanel(root.transform, "RunCard", CozyPanelStyle.Card, UITheme.Panel);
            UIFactory.Centered(card.rectTransform, Vector2.zero, new Vector2(660f, 520f));

            var title = UIFactory.CreateText(
                card.transform, "Title", "RUN OVER", 64, TextWeight.Bold,
                TextAnchor.MiddleCenter, UITheme.Text, tracked: true);
            UIFactory.Centered(title.rectTransform, new Vector2(0f, 158f), new Vector2(580f, 92f));
            title.gameObject.AddComponent<UIGradient>().SetColors(UITheme.Text, UITheme.WithAlpha(UITheme.Amber, 0.85f));

            reachedText = UIFactory.CreateText(
                card.transform, "ReachedText", "REACHED WAVE 01", 24, TextWeight.Regular,
                TextAnchor.MiddleCenter, UITheme.Muted, tracked: true);
            UIFactory.Centered(reachedText.rectTransform, new Vector2(0f, 92f), new Vector2(520f, 32f));

            var restart = UIFactory.CreatePillButton(card.transform, "RestartButton", "RESTART", true);
            UIFactory.Centered(restart.GetComponent<RectTransform>(), new Vector2(0f, 2f), new Vector2(360f, 68f));
            restart.gameObject.AddComponent<RestartButton>();

            returnButton = UIFactory.CreatePillButton(card.transform, "MainMenuButton", "RETURN TO MAIN MENU", false);
            UIFactory.Centered(returnButton.GetComponent<RectTransform>(), new Vector2(0f, -82f), new Vector2(360f, 68f));

            var hint = UIFactory.CreateText(
                card.transform, "Hint", "PRESS R TO RESTART", 14, TextWeight.Regular,
                TextAnchor.MiddleCenter, UITheme.Faint, tracked: true);
            UIFactory.Centered(hint.rectTransform, new Vector2(0f, -160f), new Vector2(420f, 22f));
        }
    }
}
