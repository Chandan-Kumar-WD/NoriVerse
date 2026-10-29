using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NoriVerse.Audio;
using UnityEditor;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>
    /// Imports the processed clips in Assets/_Project/Audio with mobile settings and writes the AudioLibrary the
    /// AudioDirector loads from Resources. Batch mode: -executeMethod NoriVerse.EditorTools.AudioLibraryBuilder.Build
    /// </summary>
    public static class AudioLibraryBuilder
    {
        private const string AudioRoot = "Assets/_Project/Audio";
        private const string LibraryAsset = "Assets/_Project/Resources/" + AudioLibrary.ResourcePath + ".asset";

        // Mix levels relative to the loudness-normalised files (Tools/AudioPipeline/process_audio.py).
        private static readonly Dictionary<string, float> FolderVolume = new Dictionary<string, float>
        {
            { "Music", 0.38f }, { "Voice", 1.0f }, { "SFX", 0.75f },
        };
        private static readonly Dictionary<SoundId, float> VolumeOverride = new Dictionary<SoundId, float>
        {
            { SoundId.WhiskStirLoop, 0.55f }, { SoundId.RollingPinLoop, 0.55f }, { SoundId.DoughCutTrace, 0.5f },
            { SoundId.SauceSquishLoop, 0.55f }, { SoundId.OvenBakeLoop, 0.5f }, { SoundId.CheeseSprinkle, 0.45f },
            { SoundId.ItemPopIn, 0.6f }, { SoundId.SceneWhoosh, 0.5f },
        };

        [MenuItem("NoriVerse/Build Audio Library")]
        public static void Build()
        {
            var entries = new List<AudioLibrary.Entry>();
            var missing = new List<SoundId>((SoundId[])Enum.GetValues(typeof(SoundId)));
            missing.Remove(SoundId.None);
            foreach (var folder in FolderVolume.Keys)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { $"{AudioRoot}/{folder}" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!Enum.TryParse(Pascal(Path.GetFileNameWithoutExtension(path)), out SoundId id))
                    {
                        Debug.LogWarning($"AudioLibraryBuilder: {path} has no matching SoundId.");
                        continue;
                    }
                    Configure(path, folder == "Music");
                    entries.Add(new AudioLibrary.Entry
                    {
                        id = id,
                        clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path),
                        volume = VolumeOverride.TryGetValue(id, out float v) ? v : FolderVolume[folder],
                    });
                    missing.Remove(id);
                }
            }
            if (missing.Count > 0) Debug.LogWarning("AudioLibraryBuilder: no clip for " + string.Join(", ", missing));

            Directory.CreateDirectory(Path.GetDirectoryName(LibraryAsset));
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryAsset);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, LibraryAsset);
            }
            library.SetEntries(entries.ToArray());
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log($"Audio library: {entries.Count} clips, {missing.Count} missing.");
        }

        private static void Configure(string path, bool isMusic)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            importer.loadInBackground = isMusic;
            var settings = importer.defaultSampleSettings;
            settings.loadType = isMusic ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = isMusic ? 0.6f : 0.7f;
            settings.preloadAudioData = !isMusic;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }

        private static string Pascal(string snake)
        {
            var sb = new StringBuilder();
            foreach (var part in snake.Split('_'))
                if (part.Length > 0) sb.Append(char.ToUpperInvariant(part[0])).Append(part.Substring(1));
            return sb.ToString();
        }
    }
}
