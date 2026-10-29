using UnityEditor;
using UnityEditor.SceneManagement;

namespace NoriVerse.EditorTools
{
    /// <summary>Opens the splash scene and presses Play. Menu: NoriVerse/Run Splash. Also usable with -executeMethod at launch.</summary>
    public static class SplashRunner
    {
        [MenuItem("NoriVerse/Run Splash")]
        public static void OpenAndPlay()
        {
            EditorSceneManager.OpenScene(SplashSceneBuilder.ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
    }
}
