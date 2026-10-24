using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Blackhole.Tests
{
    public class GeneratedAssetsTests
    {
        [Test]
        public void MainScene_Exists()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Main.unity");
            Assert.IsNotNull(scene);
        }

        [Test]
        public void EnemyPrefabs_Exist()
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Cube.prefab"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Sphere.prefab"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Triangle.prefab"));
        }

        [Test]
        public void DebrisPrefabs_Exist()
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Debris/Debris_Cube.prefab"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Debris/Debris_Sphere.prefab"));
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Debris/Debris_Shard.prefab"));
        }

        [Test]
        public void MainScene_IsFirstEnabledBuildScene()
        {
            Assert.IsTrue(EditorBuildSettings.scenes.Any(scene =>
                scene.enabled && scene.path == "Assets/Scenes/Main.unity"));
        }

        [Test]
        public void GameplayLayers_Exist()
        {
            Assert.GreaterOrEqual(LayerMask.NameToLayer("Player"), 0);
            Assert.GreaterOrEqual(LayerMask.NameToLayer("Debris"), 0);
            Assert.GreaterOrEqual(LayerMask.NameToLayer("Enemy"), 0);
        }
    }
}
