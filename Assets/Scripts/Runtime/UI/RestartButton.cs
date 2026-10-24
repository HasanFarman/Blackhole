using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    public class RestartButton : MonoBehaviour
    {
        private void Start()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(Restart);
            }
        }

        public void Restart()
        {
            RunManager.Instance?.Restart();
        }
    }
}
