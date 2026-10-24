using UnityEngine;

namespace Blackhole
{
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float seconds = 2.5f;

        public void Configure(float lifetime)
        {
            seconds = Mathf.Max(0.05f, lifetime);
        }

        private void Start()
        {
            Destroy(gameObject, seconds);
        }
    }
}
