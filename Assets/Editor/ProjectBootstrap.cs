using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Blackhole.Editor
{
    public static class ProjectBootstrap
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";
        private const string MaterialsFolder = "Assets/Materials";
        private const string MeshesFolder = "Assets/Meshes";
        private const string DebrisPrefabFolder = "Assets/Prefabs/Debris";
        private const string EnemyPrefabFolder = "Assets/Prefabs/Enemies";

        [MenuItem("Blackhole/Rebuild Main Scene")]
        public static void BuildAll()
        {
            var playerLayer = EnsureLayer("Player");
            var debrisLayer = EnsureLayer("Debris");
            var enemyLayer = EnsureLayer("Enemy");

            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialsFolder);
            EnsureFolder(MeshesFolder);
            EnsureFolder("Assets/Prefabs");
            EnsureFolder(DebrisPrefabFolder);
            EnsureFolder(EnemyPrefabFolder);

            var playerMaterial = CreateMaterial(
                $"{MaterialsFolder}/PlayerBlack.mat",
                new Color(0.008f, 0.008f, 0.012f),
                0.9f,
                0.15f,
                Color.black);
            var debrisMaterial = CreateMaterial(
                $"{MaterialsFolder}/Debris.mat",
                new Color(0.58f, 0.86f, 1f),
                0.7f,
                0.1f,
                new Color(0.06f, 0.2f, 0.35f));
            var enemyMaterial = CreateMaterial(
                $"{MaterialsFolder}/EnemyRed.mat",
                new Color(0.8f, 0.055f, 0.055f),
                0.35f,
                0.05f,
                new Color(0.18f, 0.005f, 0.005f));
            var groundMaterial = CreateMaterial(
                $"{MaterialsFolder}/Ground.mat",
                new Color(0.018f, 0.02f, 0.034f),
                0.05f,
                0f,
                Color.black);

            if (enemyMaterial.HasProperty("_Cull"))
            {
                enemyMaterial.SetFloat("_Cull", 0f);
            }

            var prismMesh = CreateTriangularPrismMesh();

            var debrisPrefabs = new List<GameObject>
            {
                CreateDebrisPrefab("Debris_Cube", $"{DebrisPrefabFolder}/Debris_Cube.prefab", PrimitiveType.Cube, new Vector3(0.35f, 0.35f, 0.35f), debrisLayer, debrisMaterial),
                CreateDebrisPrefab("Debris_Sphere", $"{DebrisPrefabFolder}/Debris_Sphere.prefab", PrimitiveType.Sphere, new Vector3(0.3f, 0.3f, 0.3f), debrisLayer, debrisMaterial),
                CreateDebrisPrefab("Debris_Shard", $"{DebrisPrefabFolder}/Debris_Shard.prefab", PrimitiveType.Cube, new Vector3(0.16f, 0.12f, 0.5f), debrisLayer, debrisMaterial)
            };

            var enemyPrefabs = new List<GameObject>
            {
                CreateEnemyPrefab("Enemy_Cube", $"{EnemyPrefabFolder}/Enemy_Cube.prefab", enemyLayer, enemyMaterial, PrimitiveType.Cube, Vector3.one),
                CreateEnemyPrefab("Enemy_Sphere", $"{EnemyPrefabFolder}/Enemy_Sphere.prefab", enemyLayer, enemyMaterial, PrimitiveType.Sphere, Vector3.one * 1.05f),
                CreateEnemyPrefab("Enemy_Triangle", $"{EnemyPrefabFolder}/Enemy_Triangle.prefab", enemyLayer, enemyMaterial, null, Vector3.one, prismMesh)
            };

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.09f, 0.1f, 0.15f);
            RenderSettings.fog = false;

            var player = CreatePlayer(playerLayer, debrisLayer, playerMaterial);
            var orbit = player.GetComponent<BlackHoleOrbit>();
            var controller = player.GetComponent<BlackHoleController>();
            var body = player.GetComponent<Rigidbody>();

            var camera = CreateCamera(player.transform);
            controller.Configure(orbit, body, camera, 7f, 40f, 20f, 0.12f);

            CreateGround(groundMaterial);
            CreateLighting();

            var manager = new GameObject("GameManager");
            var debrisField = manager.AddComponent<DebrisField>();
            var spawner = manager.AddComponent<EnemySpawner>();
            var runManager = manager.AddComponent<RunManager>();

            debrisField.Configure(debrisPrefabs.ToArray(), orbit, player.transform, 48);
            spawner.Configure(player.transform, debrisField, runManager, enemyPrefabs.ToArray(), 13f, 17f);

            var hud = CreateHud(spawner, orbit);
            runManager.Configure(controller, spawner, debrisField, hud, 6);

            EditorSceneManager.SaveScene(scene, MainScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Blackhole BuildAll complete: " + MainScenePath);
        }

        private static GameObject CreatePlayer(int playerLayer, int debrisLayer, Material playerMaterial)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Player";
            player.tag = "Player";
            player.layer = playerLayer;
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.transform.localScale = Vector3.one * 1.4f;
            player.GetComponent<Renderer>().sharedMaterial = playerMaterial;

            var body = player.AddComponent<Rigidbody>();
            body.mass = 5f;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var orbit = player.AddComponent<BlackHoleOrbit>();
            orbit.Configure(player.transform, 4.5f, 2.2f, 3.8f, 120f);

            var well = player.AddComponent<GravityWell>();
            well.Configure(9f, 22f, 1 << debrisLayer);

            player.AddComponent<BlackHoleController>();

            var aura = new GameObject("Aura");
            aura.transform.SetParent(player.transform, false);
            var light = aura.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.35f, 0.45f, 1f);
            light.intensity = 2.2f;
            light.range = 7f;

            return player;
        }

        private static Camera CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.002f, 0.003f, 0.008f);
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;
            cameraObject.AddComponent<AudioListener>();

            var follow = cameraObject.AddComponent<CameraFollow>();
            follow.Configure(target, 65f, 14f, 8f);
            follow.SnapToTarget();

            return camera;
        }

        private static void CreateGround(Material groundMaterial)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(100f, 1f, 100f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var collider = ground.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.92f, 0.95f, 1f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static HUD CreateHud(EnemySpawner spawner, BlackHoleOrbit orbit)
        {
            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var waveText = CreateText(
                canvasObject.transform, "WaveText", "Wave 1", 34, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -24f), new Vector2(420f, 48f));
            var enemyText = CreateText(
                canvasObject.transform, "EnemyText", "Enemies 0", 26, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -72f), new Vector2(420f, 40f));
            var debrisText = CreateText(
                canvasObject.transform, "DebrisText", "Debris 0", 28, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-28f, -24f), new Vector2(420f, 44f));
            var breakText = CreateText(
                canvasObject.transform, "BreakText", "Next wave in 5", 42, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 110f), new Vector2(800f, 70f));
            breakText.gameObject.SetActive(false);

            CreateText(
                canvasObject.transform, "ControlsText",
                "WASD move   |   Mouse aim   |   Left click throw   |   R restart",
                20, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 20f), new Vector2(1100f, 40f));

            var gameOverPanel = new GameObject("GameOverPanel", typeof(Image));
            gameOverPanel.transform.SetParent(canvasObject.transform, false);
            var panelRect = gameOverPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

            var gameOverText = CreateText(
                gameOverPanel.transform, "GameOverText", "", 56, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 80f), new Vector2(1000f, 300f));

            var restartObject = new GameObject("RestartButton", typeof(Image), typeof(Button));
            restartObject.transform.SetParent(gameOverPanel.transform, false);
            var restartRect = restartObject.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0.5f, 0.5f);
            restartRect.anchorMax = new Vector2(0.5f, 0.5f);
            restartRect.pivot = new Vector2(0.5f, 0.5f);
            restartRect.anchoredPosition = new Vector2(0f, -120f);
            restartRect.sizeDelta = new Vector2(280f, 70f);

            var restartImage = restartObject.GetComponent<Image>();
            restartImage.color = new Color(0.17f, 0.2f, 0.28f, 0.98f);
            var button = restartObject.GetComponent<Button>();
            button.targetGraphic = restartImage;
            restartObject.AddComponent<RestartButton>();

            CreateText(
                restartObject.transform, "RestartLabel", "Restart", 32, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);

            gameOverPanel.SetActive(false);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            var inputModule = eventSystem.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            var hud = canvasObject.AddComponent<HUD>();
            hud.Configure(spawner, orbit, waveText, enemyText, debrisText, breakText, gameOverPanel, gameOverText);
            return hud;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.color = new Color(0.92f, 0.96f, 1f);
            text.alignment = alignment;
            text.raycastTarget = false;

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            return text;
        }

        private static GameObject CreateDebrisPrefab(
            string name,
            string path,
            PrimitiveType primitive,
            Vector3 visualScale,
            int layer,
            Material material)
        {
            var root = new GameObject(name);
            root.layer = layer;

            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.35f;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var collider = root.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = true;

            var visual = GameObject.CreatePrimitive(primitive);
            visual.name = "Visual";
            visual.layer = layer;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = visualScale;
            visual.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            root.AddComponent<Debris>();
            return SavePrefab(root, path);
        }

        private static GameObject CreateEnemyPrefab(
            string name,
            string path,
            int layer,
            Material material,
            PrimitiveType? primitive,
            Vector3 visualScale,
            Mesh prismMesh = null)
        {
            var root = new GameObject(name);
            root.layer = layer;

            var body = root.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            GameObject visual;
            if (primitive.HasValue)
            {
                visual = GameObject.CreatePrimitive(primitive.Value);
                var visualCollider = visual.GetComponent<Collider>();
                if (visualCollider != null)
                {
                    Object.DestroyImmediate(visualCollider);
                }

                if (primitive.Value == PrimitiveType.Cube)
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.size = Vector3.one;
                }
                else
                {
                    var sphere = root.AddComponent<SphereCollider>();
                    sphere.radius = 0.52f;
                }
            }
            else
            {
                visual = new GameObject("Visual");
                var filter = visual.AddComponent<MeshFilter>();
                filter.sharedMesh = prismMesh;
                visual.AddComponent<MeshRenderer>().sharedMaterial = material;

                var meshCollider = root.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = prismMesh;
                meshCollider.convex = true;
            }

            visual.name = "Visual";
            visual.layer = layer;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = visualScale;
            if (!primitive.HasValue)
            {
                visual.GetComponent<Renderer>().sharedMaterial = material;
            }
            else
            {
                visual.GetComponent<Renderer>().sharedMaterial = material;
            }

            root.AddComponent<Enemy>();
            return SavePrefab(root, path);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            DeleteAsset(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Material CreateMaterial(
            string path,
            Color color,
            float smoothness,
            float metallic,
            Color emission)
        {
            DeleteAsset(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (emission.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", emission);
                }
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Mesh CreateTriangularPrismMesh()
        {
            var path = $"{MeshesFolder}/TriangularPrism.asset";
            DeleteAsset(path);

            const float radius = 0.72f;
            const float halfHeight = 0.5f;
            var vertices = new Vector3[6];
            for (var i = 0; i < 3; i++)
            {
                var angle = Mathf.Deg2Rad * (90f + i * 120f);
                var x = Mathf.Cos(angle) * radius;
                var z = Mathf.Sin(angle) * radius;
                vertices[i] = new Vector3(x, -halfHeight, z);
                vertices[i + 3] = new Vector3(x, halfHeight, z);
            }

            var triangles = new[]
            {
                0, 2, 1,
                3, 4, 5,
                0, 1, 4, 0, 4, 3,
                1, 2, 5, 1, 5, 4,
                2, 0, 3, 2, 3, 5
            };

            var mesh = new Mesh { name = "TriangularPrism" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static int EnsureLayer(string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            var firstEmpty = -1;

            for (var i = 8; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == layerName)
                {
                    return i;
                }

                if (firstEmpty < 0 && string.IsNullOrEmpty(layer.stringValue))
                {
                    firstEmpty = i;
                }
            }

            if (firstEmpty < 0)
            {
                throw new System.InvalidOperationException("No empty user layer available for " + layerName);
            }

            layers.GetArrayElementAtIndex(firstEmpty).stringValue = layerName;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return firstEmpty;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void DeleteAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}
