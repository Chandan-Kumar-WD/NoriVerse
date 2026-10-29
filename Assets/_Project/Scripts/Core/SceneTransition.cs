using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using NoriVerse.Audio;

namespace NoriVerse.Core
{
    /// <summary>
    /// White fade between scenes (fade out, hold, load, fade in), matching the reference game. Lives across scene loads
    /// and creates its own overlay, so any scene can call <see cref="Go"/> without extra setup.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public const float FadeOutSeconds = 0.35f;
        public const float HoldSeconds = 0.70f;
        public const float FadeInSeconds = 0.30f;

        private static SceneTransition instance;

        private SpriteRenderer overlay;
        private Camera cam;
        private bool busy;

        private static SceneTransition Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("SceneTransition");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<SceneTransition>();
                instance.BuildOverlay();
                SceneManager.sceneLoaded += instance.OnSceneLoaded;
                return instance;
            }
        }

        public static bool IsBusy => instance != null && instance.busy;

        /// <summary>Fades to white, loads the scene, then fades back in. Pass null to just fade to white and stay.</summary>
        public static void Go(string sceneName) => Instance.StartCoroutine(Instance.Run(sceneName));

        /// <summary>If a scene is loaded some other way while the screen is still white, fade the white away.</summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (busy || overlay == null || overlay.color.a < 0.01f) return;
            cam = null;
            StartCoroutine(FadeInAfterLoad());
        }

        private IEnumerator FadeInAfterLoad()
        {
            busy = true;
            yield return null;
            yield return Tween.Fade(overlay, 0f, FadeInSeconds, Easing.InOutQuad);
            busy = false;
        }

        private void BuildOverlay()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var white = new Color32[16];
            for (int i = 0; i < 16; i++) white[i] = new Color32(253, 253, 253, 255);
            tex.SetPixels32(white);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

            var child = new GameObject("Overlay");
            child.transform.SetParent(transform, false);
            overlay = child.AddComponent<SpriteRenderer>();
            overlay.sprite = sprite;
            overlay.sortingOrder = 32000;
            Tween.SetAlpha(overlay, 0f);
        }

        private void LateUpdate()
        {
            if (overlay == null) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            float h = cam.orthographicSize * 2f;
            float w = h * Mathf.Max(cam.aspect, 3.2f);   // generous so it covers any screen shape
            overlay.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 1f);
            overlay.transform.localScale = new Vector3(w * 1.2f, h * 1.2f, 1f);
        }

        private IEnumerator Run(string sceneName)
        {
            if (busy) yield break;
            busy = true;
            if (!string.IsNullOrEmpty(sceneName)) AudioDirector.Play(SoundId.SceneWhoosh);
            yield return Tween.Fade(overlay, 1f, FadeOutSeconds, Easing.InOutQuad);
            if (!string.IsNullOrEmpty(sceneName))
            {
                yield return Tween.Wait(HoldSeconds);
                var op = SceneManager.LoadSceneAsync(sceneName);
                while (!op.isDone) yield return null;
                cam = null;
                yield return null;
                yield return Tween.Fade(overlay, 0f, FadeInSeconds, Easing.InOutQuad);
            }
            busy = false;
        }
    }
}
