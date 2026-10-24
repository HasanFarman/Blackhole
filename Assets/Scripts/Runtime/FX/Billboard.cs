using UnityEngine;

namespace Blackhole
{
    /// <summary>Makes a transform face the main camera (used by soft glow cards).</summary>
    public class Billboard : MonoBehaviour
    {
        private Transform target;

        private void LateUpdate()
        {
            if (target == null)
            {
                var camera = Camera.main;
                if (camera == null)
                {
                    return;
                }

                target = camera.transform;
            }

            transform.rotation = target.rotation;
        }
    }
}
