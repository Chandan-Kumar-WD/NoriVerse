using System;
using System.Linq;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>Helpers shared by the scene builders.</summary>
    public static class SceneBuilderUtil
    {
        public const string Art = ArtSpec.ArtRoot;

        public static Camera CreateCamera(float designWidth = 13f, Color32? background = null)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0, 0, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background ?? new Color32(253, 253, 253, 255);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            var fitter = go.AddComponent<CameraFramingFitter>();
            var so = new SerializedObject(fitter);
            so.FindProperty("designWidth").floatValue = designWidth;
            so.ApplyModifiedPropertiesWithoutUndo();
            return cam;
        }

        public static Sprite LoadSprite(string rel)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + rel);
            if (sprite == null) throw new InvalidOperationException("Missing sprite: " + rel);
            return sprite;
        }

        public static Transform Child(Transform parent, string name, Vector3 localPos = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        public static SpriteRenderer Sprite(Transform parent, string name, string rel, Vector3 localPos, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(rel);
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>A flat coloured rectangle (used to extend a background beyond the artwork on tall or wide screens).</summary>
        public static SpriteRenderer Fill(Transform parent, string name, Color32 color, Vector2 center, Vector2 size, int order)
        {
            var sr = Sprite(parent, name, "FX/white_pixel.png", new Vector3(center.x, center.y, 1f), order);
            sr.color = color;
            sr.transform.localScale = new Vector3(size.x / sr.sprite.bounds.size.x, size.y / sr.sprite.bounds.size.y, 1f);
            return sr;
        }

        public static SpriteRenderer Overlay(Transform parent, int order)
        {
            var sr = Sprite(parent, "WhiteOverlay", "FX/white_pixel.png", new Vector3(0, 0, -2f), order);
            sr.color = new Color32(253, 253, 253, 255);
            sr.transform.localScale = new Vector3(40f / sr.sprite.bounds.size.x, 24f / sr.sprite.bounds.size.y, 1f);
            return sr;
        }

        /// <summary>Adds the scene to the build list (keeping the others) in the order given.</summary>
        public static void RegisterScenes(params string[] scenePaths)
        {
            var existing = EditorBuildSettings.scenes.ToList();
            foreach (var p in scenePaths)
                if (System.IO.File.Exists(p) && existing.All(s => s.path != p)) existing.Add(new EditorBuildSettingsScene(p, true));
            string[] order = { "Assets/_Project/Scenes/Splash.unity", "Assets/_Project/Scenes/Kitchen.unity", "Assets/_Project/Scenes/Cooking.unity" };
            EditorBuildSettings.scenes = existing.OrderBy(s => { int i = Array.IndexOf(order, s.path); return i < 0 ? 99 : i; }).ToArray();
        }

        public static CircleCollider2D AddCircle(GameObject go, float radius)
        {
            var c = go.AddComponent<CircleCollider2D>();
            c.radius = radius;
            return c;
        }
    }
}
