using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Blackhole.Editor
{
    public static partial class ProjectBootstrap
    {
        private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";

        private sealed class MenuVisuals
        {
            public Material Core;
            public Material Disc;
            public Material Wisp;
            public Material Photon;
            public Material Halo;
            public Material Dust;
            public Material Trail;
            public Material Debris;
            public Material Spark;
            public Material Sky;
            public Mesh DiscMesh;
            public Mesh WispMesh;
            public VolumeProfile Post;
        }

        private static void BuildMenuScene(MenuVisuals visuals)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = visuals.Sky;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.07f, 0.085f, 0.14f);
            RenderSettings.fog = false;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 600f;
            cameraObject.AddComponent<AudioListener>();

            var menuCamera = cameraObject.AddComponent<MenuCamera>();
            menuCamera.Configure(new Vector3(2.9f, 1.55f, 0f), 26f, -9f, 14f, 0.9f);
            menuCamera.Snap();

            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            CreateMenuLighting();
            var hole = CreateShowcaseBlackHole(visuals, camera);
            BuildMenuInterface(menuCamera, hole);

            AttachPost(visuals.Post);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void CreateMenuLighting()
        {
            var keyObject = new GameObject("Key Light");
            var key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.83f, 0.7f);
            key.intensity = 0.55f;
            key.shadows = LightShadows.None;
            keyObject.transform.rotation = Quaternion.Euler(26f, -38f, 0f);

            var fillObject = new GameObject("Fill Light");
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.45f, 0.62f, 1f);
            fill.intensity = 0.28f;
            fill.shadows = LightShadows.None;
            fillObject.transform.rotation = Quaternion.Euler(18f, 142f, 0f);
        }

        private static MenuBlackHole CreateShowcaseBlackHole(MenuVisuals visuals, Camera camera)
        {
            var root = new GameObject("ShowcaseBlackHole");
            root.transform.position = new Vector3(2.9f, 1.55f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Horizon";
            core.transform.SetParent(visual.transform, false);
            core.transform.localScale = Vector3.one * 1.6f;
            Object.DestroyImmediate(core.GetComponent<Collider>());
            core.GetComponent<Renderer>().sharedMaterial = visuals.Core;

            var photon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            photon.name = "PhotonRim";
            photon.transform.SetParent(visual.transform, false);
            photon.transform.localScale = Vector3.one * 1.68f;
            Object.DestroyImmediate(photon.GetComponent<Collider>());
            photon.GetComponent<Renderer>().sharedMaterial = visuals.Photon;

            var halo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            halo.name = "Halo";
            halo.transform.SetParent(visual.transform, false);
            halo.transform.localPosition = new Vector3(0f, 0f, 0.1f);
            halo.transform.localScale = Vector3.one * 7.2f;
            Object.DestroyImmediate(halo.GetComponent<Collider>());
            halo.GetComponent<Renderer>().sharedMaterial = visuals.Halo;
            halo.AddComponent<Billboard>();

            var disc = CreateDisc("Disc", visuals.DiscMesh, visuals.Disc, visual.transform);
            disc.transform.localRotation = Quaternion.Euler(2f, 0f, -6f);

            var wisp = CreateDisc("Wisp", visuals.WispMesh, visuals.Wisp, visual.transform);
            wisp.transform.localRotation = Quaternion.Euler(-3f, 0f, 5f);

            var lightObject = new GameObject("Glow");
            lightObject.transform.SetParent(visual.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.45f);
            light.intensity = 2.6f;
            light.range = 16f;
            light.shadows = LightShadows.None;

            var debris = new Transform[10];
            var random = new System.Random(5);
            for (var i = 0; i < debris.Length; i++)
            {
                var primitive = i % 3 == 0 ? PrimitiveType.Sphere : PrimitiveType.Cube;
                var chunk = GameObject.CreatePrimitive(primitive);
                chunk.name = "Debris_" + i;
                chunk.transform.SetParent(root.transform, false);
                Object.DestroyImmediate(chunk.GetComponent<Collider>());
                chunk.GetComponent<Renderer>().sharedMaterial = visuals.Debris;

                var scale = 0.14f + (float)random.NextDouble() * 0.26f;
                chunk.transform.localScale = primitive == PrimitiveType.Cube
                    ? new Vector3(scale, scale * 0.7f, scale * (1.4f + (float)random.NextDouble()))
                    : Vector3.one * scale;
                debris[i] = chunk.transform;
            }

            var dust = CreateDustSystem(root, visuals.Dust, 3.1f, 26f, new Color(0.85f, 0.95f, 1f), false, 24f);
            var trail = CreateTrailSystem(root, visuals.Trail, new Color(1f, 0.8f, 0.55f));

            var hole = root.AddComponent<MenuBlackHole>();
            hole.Configure(visual.transform, debris, camera, trail);

            var animator = root.AddComponent<BlackHoleVisual>();
            animator.Configure(disc, wisp, light, 3.4f, -1.6f);

            return hole;
        }

        private static Transform CreateDisc(string name, Mesh mesh, Material material, Transform parent)
        {
            var discObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            discObject.transform.SetParent(parent, false);
            discObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            discObject.GetComponent<MeshRenderer>().sharedMaterial = material;
            discObject.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            discObject.GetComponent<MeshRenderer>().receiveShadows = false;
            return discObject.transform;
        }

        private static void BuildMenuInterface(MenuCamera menuCamera, MenuBlackHole hole)
        {
            var canvas = UIFactory.CreateCanvas("MenuCanvas", 10);
            var root = canvas.transform;

            var titleGroupObject = new GameObject("TitleGroup", typeof(RectTransform), typeof(CanvasGroup));
            titleGroupObject.transform.SetParent(root, false);
            UIFactory.Stretch(titleGroupObject.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            var titleGroup = titleGroupObject.GetComponent<CanvasGroup>();

            var titleBlockObject = new GameObject("TitleBlock", typeof(RectTransform));
            titleBlockObject.transform.SetParent(titleGroupObject.transform, false);
            var titleBlock = titleBlockObject.GetComponent<RectTransform>();
            UIFactory.SetRect(
                titleBlock,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(1400f, 420f));

            var kicker = UIFactory.CreateText(
                titleBlock, "Kicker", "A COZY GRAVITY WELL", 18, TextWeight.Regular,
                TextAnchor.UpperLeft, UITheme.Muted, tracked: true);
            UIFactory.TopLeft(kicker.rectTransform, new Vector2(84f, -66f), new Vector2(700f, 28f));

            var kickerRule = UIFactory.CreatePanel(titleBlock, "KickerRule", CozyPanelStyle.Flat, UITheme.Faint);
            UIFactory.TopLeft(kickerRule.rectTransform, new Vector2(510f, -79f), new Vector2(300f, 1f));

            var title = UIFactory.CreateText(
                titleBlock, "Title", "BLACKHOLE", 138, TextWeight.Bold,
                TextAnchor.UpperLeft, UITheme.Text, tracked: true);
            UIFactory.TopLeft(title.rectTransform, new Vector2(66f, -94f), new Vector2(1300f, 200f));
            title.gameObject.AddComponent<UIGradient>().SetColors(Color.white, new Color(0.66f, 0.84f, 1f));

            var tagline = UIFactory.CreateText(
                titleBlock, "Tagline", "ORBIT THE DEBRIS. THROW IT AT THE DARK.", 17, TextWeight.Regular,
                TextAnchor.UpperLeft, UITheme.WithAlpha(UITheme.Muted, 0.9f), tracked: true);
            UIFactory.TopLeft(tagline.rectTransform, new Vector2(86f, -306f), new Vector2(780f, 26f));

            var menuObject = new GameObject("Menu", typeof(RectTransform));
            menuObject.transform.SetParent(root, false);
            var menuRect = menuObject.GetComponent<RectTransform>();
            UIFactory.SetRect(
                menuRect,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(84f, 118f), new Vector2(480f, 198f));

            var entries = new[]
            {
                UIFactory.CreateMenuEntry(menuObject.transform, "PlayEntry", "PLAY", "01", MenuAction.Play),
                UIFactory.CreateMenuEntry(menuObject.transform, "HowToEntry", "HOW TO PLAY", "02", MenuAction.HowToPlay),
                UIFactory.CreateMenuEntry(menuObject.transform, "QuitEntry", "QUIT", "03", MenuAction.Quit)
            };

            for (var i = 0; i < entries.Length; i++)
            {
                UIFactory.SetRect(
                    entries[i].GetComponent<RectTransform>(),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(0f, -i * 66f), new Vector2(480f, 66f));
            }

            var version = UIFactory.CreateText(
                root, "Version", "BUILD 0.4  ·  MADE WITH GRAVITY", 12, TextWeight.Regular,
                TextAnchor.LowerLeft, UITheme.Faint, tracked: true);
            UIFactory.SetRect(
                version.rectTransform,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(86f, 38f), new Vector2(600f, 22f));

            var footer = UIFactory.CreateText(
                root, "Footer", "ORBIT  ·  GATHER  ·  THROW", 13, TextWeight.Regular,
                TextAnchor.LowerRight, UITheme.Faint, tracked: true);
            UIFactory.SetRect(
                footer.rectTransform,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-86f, 38f), new Vector2(600f, 22f));

            BuildHowToPanel(root, out var howToGroup, out var backButton);

            var fade = UIFactory.CreatePanel(root, "Fade", CozyPanelStyle.Flat, Color.black, true);
            UIFactory.Stretch(fade.rectTransform, 0f, 0f, 0f, 0f);
            fade.rectTransform.SetAsLastSibling();
            var screenFade = fade.gameObject.AddComponent<ScreenFade>();
            screenFade.Configure(fade);

            var controller = canvas.gameObject.AddComponent<MenuController>();
            controller.Configure(titleGroup, titleBlock, entries, screenFade, howToGroup, backButton, menuCamera);
        }

        private static void BuildHowToPanel(Transform parent, out CanvasGroup group, out Button backButton)
        {
            var panelObject = new GameObject("HowTo", typeof(RectTransform), typeof(CanvasGroup));
            panelObject.transform.SetParent(parent, false);
            UIFactory.Stretch(panelObject.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            group = panelObject.GetComponent<CanvasGroup>();

            var dim = UIFactory.CreatePanel(panelObject.transform, "Dim", CozyPanelStyle.Flat, new Color(0.008f, 0.013f, 0.03f, 0.72f), true);
            UIFactory.Stretch(dim.rectTransform, 0f, 0f, 0f, 0f);
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            var card = UIFactory.CreatePanel(panelObject.transform, "HowToCard", CozyPanelStyle.Card, UITheme.Panel);
            UIFactory.Centered(card.rectTransform, Vector2.zero, new Vector2(820f, 640f));

            var title = UIFactory.CreateText(
                card.transform, "Title", "HOW TO PLAY", 46, TextWeight.Bold,
                TextAnchor.MiddleCenter, UITheme.Text, tracked: true);
            UIFactory.Centered(title.rectTransform, new Vector2(0f, 236f), new Vector2(700f, 64f));
            title.gameObject.AddComponent<UIGradient>().SetColors(UITheme.Text, UITheme.WithAlpha(UITheme.Muted, 0.8f));

            var subtitle = UIFactory.CreateText(
                card.transform, "Subtitle", "ONE GOOD THROW IS ALL IT TAKES", 14, TextWeight.Regular,
                TextAnchor.MiddleCenter, UITheme.Muted, tracked: true);
            UIFactory.Centered(subtitle.rectTransform, new Vector2(0f, 192f), new Vector2(700f, 24f));

            CreateControlRow(card.transform, "WASD", "DRIFT AROUND THE FIELD", 112f);
            CreateControlRow(card.transform, "MOUSE", "AIM WHERE YOU WANT TO THROW", 52f);
            CreateControlRow(card.transform, "LEFT CLICK", "HURL A PIECE OF DEBRIS", -8f);
            CreateControlRow(card.transform, "ESC", "PAUSE THE RUN", -68f);
            CreateControlRow(card.transform, "R", "RESTART AFTER THE RUN ENDS", -128f);

            backButton = UIFactory.CreatePillButton(card.transform, "BackButton", "BACK", false);
            UIFactory.Centered(backButton.GetComponent<RectTransform>(), new Vector2(0f, -234f), new Vector2(260f, 62f));
        }

        private static void CreateControlRow(Transform parent, string key, string description, float y)
        {
            var keyText = UIFactory.CreateText(
                parent, "Key_" + key, key, 19, TextWeight.Medium,
                TextAnchor.MiddleRight, UITheme.Cyan, tracked: true);
            UIFactory.Centered(keyText.rectTransform, new Vector2(-190f, y), new Vector2(210f, 30f));

            var descriptionText = UIFactory.CreateText(
                parent, "Desc_" + key, description, 17, TextWeight.Regular,
                TextAnchor.MiddleLeft, UITheme.Muted, tracked: true);
            UIFactory.Centered(descriptionText.rectTransform, new Vector2(130f, y), new Vector2(500f, 30f));

            var rule = UIFactory.CreatePanel(parent, "Rule_" + key, CozyPanelStyle.Flat, UITheme.WithAlpha(UITheme.Muted, 0.18f));
            UIFactory.Centered(rule.rectTransform, new Vector2(0f, y - 28f), new Vector2(620f, 1f));
        }
    }
}
