using UnityEditor;

namespace NoriVerse.EditorTools
{
    /// <summary>Rebuilds every scene in order. Batch mode: -executeMethod NoriVerse.EditorTools.BuildAllScenes.Run</summary>
    public static class BuildAllScenes
    {
        [MenuItem("NoriVerse/Build All Scenes")]
        public static void Run()
        {
            AudioLibraryBuilder.Build();
            SplashSceneBuilder.Build();
            KitchenSceneBuilder.Build();
            CookingSceneBuilder.Build();
            PizzaFlowSceneBuilder.BuildAll();
            PizzaFinishSceneBuilder.BuildAll();
        }
    }
}
