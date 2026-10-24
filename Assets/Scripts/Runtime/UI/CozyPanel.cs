using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    public enum CozyPanelStyle
    {
        Card,
        CardSoft,
        Pill,
        Glow,
        Ring,
        Flat
    }

    /// <summary>Skins a uGUI Image with one of the generated space-UI sprites.</summary>
    [RequireComponent(typeof(Image))]
    public class CozyPanel : MonoBehaviour
    {
        [SerializeField] private CozyPanelStyle style = CozyPanelStyle.Card;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private bool blockRaycasts;

        public void Configure(CozyPanelStyle panelStyle, Color panelTint, bool blocks = false)
        {
            style = panelStyle;
            tint = panelTint;
            blockRaycasts = blocks;

            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            var image = GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            switch (style)
            {
                case CozyPanelStyle.Card:
                    image.sprite = UITheme.Rounded(28);
                    image.type = Image.Type.Sliced;
                    image.color = tint;
                    break;
                case CozyPanelStyle.CardSoft:
                    image.sprite = UITheme.Rounded(28);
                    image.type = Image.Type.Sliced;
                    image.color = tint;
                    break;
                case CozyPanelStyle.Pill:
                    image.sprite = UITheme.Pill;
                    image.type = Image.Type.Sliced;
                    image.color = tint;
                    break;
                case CozyPanelStyle.Glow:
                    image.sprite = UITheme.Glow;
                    image.type = Image.Type.Simple;
                    image.color = tint;
                    break;
                case CozyPanelStyle.Ring:
                    image.sprite = UITheme.Ring;
                    image.type = Image.Type.Simple;
                    image.color = tint;
                    break;
                case CozyPanelStyle.Flat:
                    image.sprite = null;
                    image.color = tint;
                    break;
            }

            image.raycastTarget = blockRaycasts;
        }
    }
}
