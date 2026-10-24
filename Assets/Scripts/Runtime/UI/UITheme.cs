using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>
    /// Shared look for every screen: palette, typeface and the small set of
    /// procedurally generated sprites the UI skins itself with.
    /// </summary>
    public static class UITheme
    {
        public static readonly Color Night = new Color(0.012f, 0.02f, 0.047f, 1f);
        public static readonly Color Panel = new Color(0.043f, 0.071f, 0.141f, 0.86f);
        public static readonly Color PanelSoft = new Color(0.055f, 0.09f, 0.17f, 0.55f);
        public static readonly Color Text = new Color(0.918f, 0.949f, 1f, 1f);
        public static readonly Color Muted = new Color(0.561f, 0.663f, 0.796f, 1f);
        public static readonly Color Faint = new Color(0.561f, 0.663f, 0.796f, 0.35f);
        public static readonly Color Cyan = new Color(0.498f, 0.831f, 1f, 1f);
        public static readonly Color Amber = new Color(1f, 0.722f, 0.42f, 1f);
        public static readonly Color DeepBlue = new Color(0.039f, 0.067f, 0.141f, 1f);

        private static Font regular;
        private static Font medium;
        private static Font bold;

        public static Font Regular => regular != null ? regular : regular = LoadFont("Fonts/SpaceGrotesk-Regular");
        public static Font Medium => medium != null ? medium : medium = LoadFont("Fonts/SpaceGrotesk-Medium");
        public static Font Bold => bold != null ? bold : bold = LoadFont("Fonts/SpaceGrotesk-Bold");

        private static readonly Dictionary<int, Sprite> roundedSprites = new Dictionary<int, Sprite>();
        private static Sprite glowSprite;
        private static Sprite ringSprite;

        public static Sprite Pill => Rounded(48);

        public static Sprite Rounded(int radius)
        {
            if (roundedSprites.TryGetValue(radius, out var cached) && cached != null)
            {
                return cached;
            }

            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UI_Rounded_" + radius,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var half = size * 0.5f;
            var inset = Mathf.Max(0.5f, half - radius);
            var pixels = new Color32[size * size];

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var px = x + 0.5f - half;
                    var py = y + 0.5f - half;
                    var qx = Mathf.Abs(px) - inset;
                    var qy = Mathf.Abs(py) - inset;
                    var outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                    var distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    var alpha = Mathf.Clamp01(0.5f - distance);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            var border = Mathf.Max(1f, radius * 0.8f);
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            sprite.name = texture.name;

            roundedSprites[radius] = sprite;
            return sprite;
        }

        public static Sprite Glow
        {
            get
            {
                if (glowSprite != null)
                {
                    return glowSprite;
                }

                const int size = 256;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "UI_Glow",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                var pixels = new Color32[size * size];
                var half = size * 0.5f;
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                        var alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.6f);
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                glowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
                glowSprite.name = texture.name;
                return glowSprite;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (ringSprite != null)
                {
                    return ringSprite;
                }

                const int size = 256;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "UI_Ring",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                var pixels = new Color32[size * size];
                var half = size * 0.5f;
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                        var band = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.78f) / 0.14f);
                        var alpha = Mathf.Pow(band, 1.6f);
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
                ringSprite.name = texture.name;
                return ringSprite;
            }
        }

        /// <summary>Approximates letter spacing (legacy Text has none) with hair spaces.</summary>
        public static string Track(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new System.Text.StringBuilder(text.Length * 2);
            for (var i = 0; i < text.Length; i++)
            {
                builder.Append(text[i]);
                if (i < text.Length - 1 && text[i] != ' ' && text[i + 1] != ' ')
                {
                    builder.Append('\u200A');
                }
            }

            return builder.ToString();
        }

        private static Font LoadFont(string resourcePath)
        {
            var font = Resources.Load<Font>(resourcePath);
            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>Multiplies a colour's alpha, keeping its RGB untouched.</summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a *= alpha;
            return color;
        }
    }
}
