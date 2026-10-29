using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    public static class ProjectAudit
    {
        [Serializable] private class Audit
        {
            public string[] sceneRoots,dependencies,unusedArt;
        }
        public static void Run()
        {
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            var dependencies=AssetDatabase.GetDependencies(scenes,true).Distinct().OrderBy(x=>x).ToArray();
            var used=new System.Collections.Generic.HashSet<string>(dependencies);
            var unused=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/_Project/Art/")&&File.Exists(p)&&!p.EndsWith(".json")&&!used.Contains(p)).OrderBy(p=>p).ToArray();
            Directory.CreateDirectory("BuildReports");
            File.WriteAllText("BuildReports/asset-audit.json",JsonUtility.ToJson(new Audit{sceneRoots=scenes,dependencies=dependencies,unusedArt=unused},true));
            Debug.Log($"Project audit: {scenes.Length} scenes, {dependencies.Length} dependencies, {unused.Length} unused art files.");
        }
    }
}
