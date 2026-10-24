using UnityEngine;

namespace Blackhole
{
    /// <summary>Spins the accretion discs and lets the core light breathe.</summary>
    public class BlackHoleVisual : MonoBehaviour
    {
        [SerializeField] private Transform mainDisc;
        [SerializeField] private Transform outerDisc;
        [SerializeField] private Light glow;
        [SerializeField] private float mainSpin = 9f;
        [SerializeField] private float outerSpin = -4f;

        private float baseIntensity;

        public void Configure(Transform innerDisc, Transform wisp, Light coreLight, float innerSpin = 9f, float wispSpin = -4f)
        {
            mainDisc = innerDisc;
            outerDisc = wisp;
            glow = coreLight;
            mainSpin = innerSpin;
            outerSpin = wispSpin;
        }

        private void Awake()
        {
            if (glow != null)
            {
                baseIntensity = glow.intensity;
            }
        }

        private void Update()
        {
            var delta = Time.deltaTime;

            if (mainDisc != null)
            {
                mainDisc.Rotate(Vector3.up, mainSpin * delta, Space.Self);
            }

            if (outerDisc != null)
            {
                outerDisc.Rotate(Vector3.up, outerSpin * delta, Space.Self);
            }

            if (glow != null)
            {
                glow.intensity = baseIntensity * (0.92f + 0.08f * Mathf.Sin(Time.time * 1.6f));
            }
        }
    }
}
