using UnityEngine;

namespace Blackhole
{
    /// <summary>Global frame cap so the game stays gentle on laptops.</summary>
    public static class RuntimeSettings
    {
        public const int TargetFrameRate = 30;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
