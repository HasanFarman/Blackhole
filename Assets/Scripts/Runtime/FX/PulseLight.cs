using UnityEngine;

namespace Blackhole
{
    /// <summary>Quick one-shot light flash attached to death bursts.</summary>
    [RequireComponent(typeof(Light))]
    public class PulseLight : MonoBehaviour
    {
        [SerializeField] private Light target;
        [SerializeField] private float duration = 0.22f;
        [SerializeField] private float peak = 5f;

        private float timer;
        private float baseIntensity;

        public void Configure(float flashDuration, float peakIntensity)
        {
            duration = Mathf.Max(0.02f, flashDuration);
            peak = Mathf.Max(0f, peakIntensity);
        }

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<Light>();
            }

            baseIntensity = peak;
            timer = duration;
        }

        private void Update()
        {
            if (target == null || timer <= 0f)
            {
                return;
            }

            timer -= Time.deltaTime;
            var t = Mathf.Clamp01(timer / duration);
            target.intensity = baseIntensity * t * t;
        }
    }
}
