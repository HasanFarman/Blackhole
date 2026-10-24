using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Pill button with a soft hover/press animation, tuned for unscaled menu time.</summary>
    [RequireComponent(typeof(Image))]
    public class CozyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private bool primary;
        [SerializeField] private Color primaryFill = new Color(0.62f, 0.87f, 1f, 1f);
        [SerializeField] private Color primaryHover = new Color(0.79f, 0.94f, 1f, 1f);
        [SerializeField] private Color primaryLabelColor = new Color(0.02f, 0.07f, 0.12f, 1f);
        [SerializeField] private Color ghostFill = new Color(1f, 1f, 1f, 0.07f);
        [SerializeField] private Color ghostHover = new Color(0.7f, 0.88f, 1f, 0.16f);

        private Image background;
        private float hover;
        private float press;
        private bool hovered;
        private bool pressed;
        private Vector3 baseScale;

        public void Configure(Text labelText, bool isPrimary)
        {
            label = labelText;
            primary = isPrimary;
            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void Awake()
        {
            background = GetComponent<Image>();
            if (label == null)
            {
                label = GetComponentInChildren<Text>(true);
            }

            baseScale = transform.localScale;
            Apply();

            if (background != null)
            {
                background.color = primary ? primaryFill : ghostFill;
            }

            if (label != null)
            {
                label.color = primary ? primaryLabelColor : UITheme.Text;
            }
        }

        private void Apply()
        {
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (background == null)
            {
                return;
            }

            background.sprite = UITheme.Pill;
            background.type = Image.Type.Sliced;
            background.raycastTarget = true;
        }

        private void Update()
        {
            var step = Time.unscaledDeltaTime * 8f;
            hover = Mathf.MoveTowards(hover, hovered ? 1f : 0f, step);
            press = Mathf.MoveTowards(press, pressed ? 1f : 0f, Time.unscaledDeltaTime * 18f);

            if (background != null)
            {
                background.color = primary
                    ? Color.Lerp(primaryFill, primaryHover, hover)
                    : Color.Lerp(ghostFill, ghostHover, hover);
            }

            if (label != null)
            {
                label.color = primary
                    ? primaryLabelColor
                    : Color.Lerp(new Color(0.82f, 0.89f, 0.98f, 1f), Color.white, hover);
            }

            transform.localScale = baseScale * (1f + 0.025f * hover - 0.02f * press);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }
    }
}
