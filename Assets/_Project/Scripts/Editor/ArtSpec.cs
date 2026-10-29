using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>
    /// Pixels-per-unit and pivot for every splash sprite, chosen so each one imports at its final on-screen size
    /// (Unity units, camera orthographic size 5 = 10 units tall). Shared by the importer and the scene builder.
    /// </summary>
    public static class ArtSpec
    {
        public const string ArtRoot = "Assets/_Project/Art/";

        // Full-body Nori: 584 px head width -> 2.5 u. Feet pivot (550 of 1100 px, 40 px above the bottom of 1280).
        public const float NoriBodyPpu = 233.6f;
        public static readonly Vector2 NoriBodyPivot = new Vector2(0.5f, 40f / 1280f);

        // Peek: 600 px face width -> 2.67 u. Bottom-centre pivot sits on the screen edge.
        public const float PeekPpu = 210f;
        public static readonly Vector2 PeekPivot = new Vector2(0.5f, 0f);
        public const int PeekWidthPx = 690;
        public const int PeekHeightPx = 896;

        // Raised forearm + paw: 600 px tall -> 1.25 u. Bottom-centre pivot.
        public const float HandPpu = 480f;

        public const float LogoPpu = 300f;

        [System.Serializable] private class LogoLetter { public string sprite; public float x; public float pivotY; }
        [System.Serializable] private class LogoLayoutFile { public List<LogoLetter> letters = new List<LogoLetter>(); }

        [System.Serializable] private class KitchenSprite { public string path; public float ppu; public float pivotX; public float pivotY; }
        [System.Serializable] private class KitchenLayoutFile { public List<KitchenSprite> sprites = new List<KitchenSprite>(); }

        private static Dictionary<string, KitchenSprite> kitchenLayout;

        /// <summary>Per-sprite size/pivot written by process_kitchen_art.py (Video 002 art).</summary>
        private static bool TryGetKitchen(string assetPath, out float ppu, out Vector2 pivot)
        {
            ppu = 100f; pivot = new Vector2(0.5f, 0.5f);
            if (kitchenLayout == null)
            {
                kitchenLayout = new Dictionary<string, KitchenSprite>();
                string json = Path.Combine(Application.dataPath, "_Project/Art/Kitchen/layout.json");
                if (File.Exists(json))
                    foreach (var s in JsonUtility.FromJson<KitchenLayoutFile>(File.ReadAllText(json)).sprites) kitchenLayout[s.path] = s;
            }
            string key = assetPath.Substring(ArtRoot.Length);
            if (!kitchenLayout.TryGetValue(key, out var entry)) return false;
            ppu = entry.ppu; pivot = new Vector2(entry.pivotX, entry.pivotY);
            return true;
        }

        public static bool TryGet(string assetPath, out float ppu, out Vector2 pivot)
        {
            ppu = 100f;
            pivot = new Vector2(0.5f, 0.5f);
            if (TryGetKitchen(assetPath, out ppu, out pivot)) return true;
            ppu = 100f;
            pivot = new Vector2(0.5f, 0.5f);
            string file = Path.GetFileNameWithoutExtension(assetPath);

            if (assetPath.Contains("/Characters/Nori/"))
            {
                if (file == "nori_hand_up") { ppu = HandPpu; pivot = new Vector2(0.5f, 0f); return true; }
                if (file.StartsWith("nori_peek")) { ppu = PeekPpu; pivot = PeekPivot; return true; }
                if (file.StartsWith("nori_")) { ppu = NoriBodyPpu; pivot = NoriBodyPivot; return true; }
            }
            switch (file)
            {
                case "table": ppu = 234.1f; return true;                                  // 1077 px -> 4.6 u wide
                case "cloche": ppu = 503f; pivot = new Vector2(0.5f, 0f); return true;    // 835 px -> 1.66 u
                case "noodle_bowl": ppu = 738f; pivot = new Vector2(0.5f, 0f); return true; // 716 px -> 0.97 u
                case "tagline": ppu = LogoPpu; return true;
            }
            if (assetPath.Contains("/UI/Logo/") && file.StartsWith("logo_"))
            {
                ppu = LogoPpu;
                pivot = new Vector2(0.5f, LogoPivotY(file));
                return true;
            }
            return true;
        }

        private static float LogoPivotY(string spriteName)
        {
            string json = Path.Combine(Application.dataPath, "_Project/Art/UI/Logo/logo_layout.json");
            if (!File.Exists(json)) return 0.5f;
            var layout = JsonUtility.FromJson<LogoLayoutFile>(File.ReadAllText(json));
            foreach (var l in layout.letters)
                if (Path.GetFileNameWithoutExtension(l.sprite) == spriteName) return l.pivotY;
            return 0.5f;
        }

        public static List<(string sprite, float x)> LogoLetters()
        {
            string json = Path.Combine(Application.dataPath, "_Project/Art/UI/Logo/logo_layout.json");
            var layout = JsonUtility.FromJson<LogoLayoutFile>(File.ReadAllText(json));
            var result = new List<(string, float)>();
            foreach (var l in layout.letters) result.Add((l.sprite, l.x));
            return result;
        }
    }
}
