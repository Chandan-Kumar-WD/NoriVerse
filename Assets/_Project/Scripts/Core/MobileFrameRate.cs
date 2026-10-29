using UnityEngine;

namespace NoriVerse.Core
{
    internal static class MobileFrameRate
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            if(Application.isMobilePlatform)Application.targetFrameRate=60;
        }
    }
}
