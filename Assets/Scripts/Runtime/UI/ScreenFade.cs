using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Full-screen black curtain used for scene reveals and transitions.</summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFade : MonoBehaviour
    {
        [SerializeField] private Image image;

        public static ScreenFade Instance { get; private set; }
        public float Alpha => image != null ? image.color.a : 0f;

        public void Configure(Image fadeImage)
        {
            image = fadeImage;
        }

        private void Awake()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            Instance = this;
            SetAlpha(1f);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (image == null)
            {
                return;
            }

            var color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
            image.raycastTarget = color.a > 0.001f;
        }

        public IEnumerator FadeTo(float target, float duration)
        {
            var start = Alpha;
            if (duration <= 0f)
            {
                SetAlpha(target);
                yield break;
            }

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t)));
                yield return null;
            }

            SetAlpha(target);
        }
    }
}
