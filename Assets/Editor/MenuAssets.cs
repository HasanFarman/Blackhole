using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Blackhole.Editor
{
    public static partial class ProjectBootstrap
    {
        private const string TextureFolder = "Assets/Textures";
        private const string ShaderAdditive = "Blackhole/Additive";

        private static Texture2D CreateSoftCircleTexture()
        {
            EnsureFolder(TextureFolder);
            const string path = TextureFolder + "/SoftCircle.png";
            const int size = 128;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var half = size * 0.5f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                    var alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return SaveTexture(texture, path, TextureWrapMode.Clamp);
        }

        private static Texture2D CreateAccretionTexture(string fileName, Color inner, Color mid, Color outer, int seed, float detail = 1f)
        {
            EnsureFolder(TextureFolder);
            const int width = 512;
            const int height = 128;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            var random = new System.Random(seed);

            var phases = new float[8];
            var frequencies = new float[8];
            for (var i = 0; i < phases.Length; i++)
            {
                phases[i] = (float)random.NextDouble() * Mathf.PI * 2f;
                frequencies[i] = 2f + (float)random.NextDouble() * 14f;
            }

            var streakAmplitude = Mathf.Lerp(0.08f, 0.5f, detail);
            var shimmerAmplitude = Mathf.Lerp(0.06f, 0.2f, detail);
            var shimmerCycles = Mathf.Lerp(7f, 22f, detail);

            for (var y = 0; y < height; y++)
            {
                var t = y / (float)(height - 1);
                var radial = Mathf.Pow(1f - t, 1.35f) * Mathf.Clamp01(t * 12f + 0.15f);

                for (var x = 0; x < width; x++)
                {
                    var angle = x / (float)width * Mathf.PI * 2f;
                    var streak = 0f;
                    for (var i = 0; i < phases.Length; i++)
                    {
                        streak += Mathf.Sin(angle * frequencies[i] + phases[i]) / phases.Length;
                    }

                    var shimmer = 1f - shimmerAmplitude +
                                  shimmerAmplitude * Mathf.Sin(angle * shimmerCycles + Mathf.Sin(angle * 4f) * 2f);
                    var brightness = Mathf.Clamp01((0.86f - streakAmplitude * 0.5f + streakAmplitude * streak) * shimmer);
                    var color = t < 0.45f
                        ? Color.Lerp(inner, mid, t / 0.45f)
                        : Color.Lerp(mid, outer, (t - 0.45f) / 0.55f);

                    var alpha = Mathf.Clamp01(radial * brightness);
                    pixels[y * width + x] = new Color32(
                        (byte)(color.r * 255f),
                        (byte)(color.g * 255f),
                        (byte)(color.b * 255f),
                        (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return SaveTexture(texture, $"{TextureFolder}/{fileName}.png", TextureWrapMode.Repeat);
        }

        private static Texture2D CreateSkyboxTexture()
        {
            EnsureFolder(TextureFolder);
            const string path = TextureFolder + "/MenuSky.png";
            const int width = 2048;
            const int height = 1024;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];

            var random = new System.Random(11);
            var blobs = new Vector4[18];
            for (var i = 0; i < blobs.Length; i++)
            {
                blobs[i] = new Vector4(
                    (float)random.NextDouble() * width,
                    height * (0.2f + (float)random.NextDouble() * 0.7f),
                    120f + (float)random.NextDouble() * 420f,
                    (float)random.NextDouble());
            }

            var top = new Color(0.012f, 0.018f, 0.04f);
            var horizon = new Color(0.03f, 0.043f, 0.09f);
            var bottom = new Color(0.008f, 0.014f, 0.032f);

            for (var y = 0; y < height; y++)
            {
                var v = y / (float)(height - 1);
                var sky = v > 0.5f
                    ? Color.Lerp(horizon, top, (v - 0.5f) * 2f)
                    : Color.Lerp(bottom, horizon, v * 2f);

                for (var x = 0; x < width; x++)
                {
                    var color = sky;

                    for (var i = 0; i < blobs.Length; i++)
                    {
                        var blob = blobs[i];
                        var dx = Mathf.Abs(x - blob.x);
                        dx = Mathf.Min(dx, width - dx);
                        var dy = y - blob.y;
                        var distance = Mathf.Sqrt(dx * dx * 0.35f + dy * dy * 1.4f) / blob.z;
                        var falloff = Mathf.Clamp01(1f - distance);
                        if (falloff <= 0f)
                        {
                            continue;
                        }

                        var weight = falloff * falloff * 0.12f;
                        var blobColor = blob.w > 0.72f
                            ? new Color(0.32f, 0.45f, 1f)
                            : blob.w > 0.36f
                                ? new Color(0.48f, 0.3f, 0.95f)
                                : new Color(1f, 0.55f, 0.3f);

                        color += blobColor * weight;
                    }

                    pixels[y * width + x] = color;
                }
            }

            for (var i = 0; i < 2400; i++)
            {
                var x = random.Next(0, width);
                var y = random.Next(0, height);
                var brightness = 0.25f + (float)random.NextDouble() * 0.75f;
                var radius = random.NextDouble() > 0.94 ? 1.6f : 0.8f;
                var tint = random.NextDouble() > 0.8
                    ? new Color(1f, 0.86f, 0.72f)
                    : new Color(0.82f, 0.9f, 1f);

                PaintStar(pixels, width, height, x, y, radius, tint * brightness);
            }

            for (var i = 0; i < 46; i++)
            {
                var x = random.Next(0, width);
                var y = random.Next(0, height);
                var brightness = 0.7f + (float)random.NextDouble() * 0.3f;
                PaintStar(pixels, width, height, x, y, 1.9f, new Color(1f, 0.95f, 0.86f) * brightness);
                PaintStar(pixels, width, height, x + 2, y, 1.1f, new Color(1f, 0.95f, 0.86f) * brightness * 0.4f);
                PaintStar(pixels, width, height, x - 2, y, 1.1f, new Color(1f, 0.95f, 0.86f) * brightness * 0.4f);
                PaintStar(pixels, width, height, x, y + 2, 1.1f, new Color(1f, 0.95f, 0.86f) * brightness * 0.4f);
                PaintStar(pixels, width, height, x, y - 2, 1.1f, new Color(1f, 0.95f, 0.86f) * brightness * 0.4f);
            }

            for (var i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i];
                pixels[i] = new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), 1f);
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return SaveTexture(texture, path, TextureWrapMode.Repeat);
        }

        private static void PaintStar(Color[] pixels, int width, int height, int x, int y, float radius, Color color)
        {
            var minX = Mathf.FloorToInt(x - radius);
            var maxX = Mathf.CeilToInt(x + radius);
            var minY = Mathf.FloorToInt(y - radius);
            var maxY = Mathf.CeilToInt(y + radius);

            for (var py = minY; py <= maxY; py++)
            {
                if (py < 0 || py >= height)
                {
                    continue;
                }

                for (var px = minX; px <= maxX; px++)
                {
                    var wrappedX = (px % width + width) % width;
                    var dx = px - x;
                    var dy = py - y;
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance > radius)
                    {
                        continue;
                    }

                    var strength = Mathf.Pow(1f - distance / radius, 1.6f);
                    var index = py * width + wrappedX;
                    pixels[index] += color * strength;
                }
            }
        }

        private static Texture2D SaveTexture(Texture2D texture, string path, TextureWrapMode wrapMode)
        {
            DeleteAsset(path);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.wrapMode = wrapMode;
                importer.mipmapEnabled = true;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material CreateAdditiveMaterial(string path, Texture2D texture, Color tint, float intensity)
        {
            DeleteAsset(path);
            var shader = Shader.Find(ShaderAdditive);
            if (shader == null)
            {
                Debug.LogError("Blackhole additive shader missing; falling back to URP unlit.");
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            var material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            if (material.HasProperty("_Intensity"))
            {
                material.SetFloat("_Intensity", intensity);
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateFresnelMaterial(string path, Color color, float power, float intensity)
        {
            DeleteAsset(path);
            var shader = Shader.Find("Blackhole/Fresnel");
            if (shader == null)
            {
                Debug.LogError("Blackhole fresnel shader missing.");
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            var material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Power"))
            {
                material.SetFloat("_Power", power);
            }

            if (material.HasProperty("_Intensity"))
            {
                material.SetFloat("_Intensity", intensity);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateSkyboxMaterial(Texture2D panorama)
        {
            const string path = MaterialsFolder + "/MenuSky.mat";
            DeleteAsset(path);

            var shader = Shader.Find("Skybox/Panoramic");
            var material = new Material(shader) { name = "MenuSky" };
            material.SetTexture("_MainTex", panorama);
            material.SetFloat("_Mapping", 1f);
            material.SetFloat("_ImageType", 0f);
            material.SetFloat("_Exposure", 1f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Mesh CreateRingMesh(string path, float innerRadius, float outerRadius, int segments)
        {
            DeleteAsset(path);

            var vertices = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * 6];

            for (var i = 0; i <= segments; i++)
            {
                var angle = i / (float)segments * Mathf.PI * 2f;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var index = i * 2;

                vertices[index] = direction * innerRadius;
                vertices[index + 1] = direction * outerRadius;
                uvs[index] = new Vector2(i / (float)segments, 0f);
                uvs[index + 1] = new Vector2(i / (float)segments, 1f);

                if (i == segments)
                {
                    continue;
                }

                var triangle = i * 6;
                triangles[triangle] = index;
                triangles[triangle + 1] = index + 1;
                triangles[triangle + 2] = index + 2;
                triangles[triangle + 3] = index + 2;
                triangles[triangle + 4] = index + 1;
                triangles[triangle + 5] = index + 3;
            }

            var mesh = new Mesh { name = Path.GetFileNameWithoutExtension(path) };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static VolumeProfile CreatePostProfile()
        {
            EnsureFolder("Assets/Settings");
            const string path = "Assets/Settings/CozyPost.asset";
            DeleteAsset(path);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.75f);
            bloom.threshold.Override(1.02f);
            bloom.scatter.Override(0.62f);

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.27f);
            vignette.smoothness.Override(0.45f);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.05f);
            color.saturation.Override(6f);
            color.contrast.Override(5f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static void AttachPost(VolumeProfile profile)
        {
            var volumeObject = new GameObject("Post");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.profile = profile;
        }
    }
}
