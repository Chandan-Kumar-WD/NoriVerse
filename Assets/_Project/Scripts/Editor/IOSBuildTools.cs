using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>Repeatable Xcode exports; signing is selected by the owner in Xcode.</summary>
    public static class IOSBuildTools
    {
        [MenuItem("NoriVerse/iOS/Export for Simulator")]
        public static void ExportSimulator()=>Export(true);
        [MenuItem("NoriVerse/iOS/Export for Device")]
        public static void ExportDevice()=>Export(false);
        [MenuItem("NoriVerse/iOS/Export for TestFlight (Release)")]
        public static void ExportTestFlight()=>Export(false,true);
        private const string AppIconPath="Assets/_Project/Art/AppIcon/app_icon_1024.png";
        /// <summary>Uses the 1024 px icon for every iOS icon slot; Unity scales it down for each size.</summary>
        private static void ApplyAppIcon()
        {
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if(icon==null)throw new InvalidOperationException("Missing app icon: "+AppIconPath);
            foreach(var kind in PlayerSettings.GetSupportedIconKindsForPlatform(BuildTargetGroup.iOS))
            {
                var slots=PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS,kind);
                foreach(var slot in slots)slot.SetTexture(icon,0);
                PlayerSettings.SetPlatformIcons(NamedBuildTarget.iOS,kind,slots);
            }
        }
        private static void PrepareTextures()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Project/Art"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)continue;
                var ios=importer.GetPlatformTextureSettings("iPhone");
                bool cpuReadable=path.EndsWith("/PizzaFlow/sauce_disc.png");
                var expected=cpuReadable?TextureImporterFormat.RGBA32:TextureImporterFormat.ASTC_4x4;
                if(ios.overridden&&ios.format==expected)continue;
                ios.name="iPhone";ios.overridden=true;ios.format=expected;ios.maxTextureSize=4096;ios.compressionQuality=100;
                ios.textureCompression=cpuReadable?TextureImporterCompression.Uncompressed:TextureImporterCompression.CompressedHQ;
                importer.SetPlatformTextureSettings(ios);importer.SaveAndReimport();
            }
        }
        private static void Export(bool simulator,bool release=false)
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))throw new InvalidOperationException("Install Unity iOS Build Support in Unity Hub.");
            PlayerSettings.companyName="NoriVerse";
            PlayerSettings.bundleVersion="1.0.0";            // marketing version; first TestFlight build is 1.0.0 (1)
            PlayerSettings.iOS.buildNumber="1";
            PlayerSettings.productName="Cooking";   // name under the icon on the device; the App Store listing is "Nori Cooking"
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.noriverse.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice=iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.targetOSVersionString="15.0";
            PlayerSettings.iOS.sdkVersion=simulator?iOSSdkVersion.SimulatorSDK:iOSSdkVersion.DeviceSDK;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            if(release)
            {
                // Apple Developer team "WorksDelight"; Xcode manages certificates and profiles.
                PlayerSettings.iOS.appleDeveloperTeamID="S8A97WCTL2";
                PlayerSettings.iOS.appleEnableAutomaticSigning=true;
            }
            ApplyAppIcon();
            PrepareTextures();
            string output=Path.GetFullPath(simulator?"Builds/iOS-Simulator":release?"Builds/iOS-TestFlight":"Builds/iOS-Device");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=BuildTarget.iOS,options=release?BuildOptions.None:BuildOptions.Development|BuildOptions.AllowDebugging
            });
            Directory.CreateDirectory("BuildReports");
            File.WriteAllText("BuildReports/ios-export.txt",$"Result: {report.summary.result}\nOutput: {output}\nSize: {report.summary.totalSize}\nErrors: {report.summary.totalErrors}\n");
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException($"iOS export failed: {report.summary.totalErrors} errors. See the Unity build log.");
            Debug.Log("Xcode project exported to "+output);
        }
    }
}
