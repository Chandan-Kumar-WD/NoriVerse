using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>
    /// Shows one of several same-sized poses and cross-fades between them instead of swapping, so expression and
    /// pose changes dissolve smoothly. The incoming sprite fades in on top of the outgoing one (which stays opaque until
    /// the end), so the background never shows through mid-fade.
    /// </summary>
    public class SpriteCrossfader : MonoBehaviour
    {
        [System.Serializable]
        public struct Pose
        {
            public string name;
            public Sprite sprite;
        }

        [SerializeField] private List<Pose> poses = new List<Pose>();
        [SerializeField] private int baseSortingOrder;
        [SerializeField] private string initialPose;

        private readonly Dictionary<string, SpriteRenderer> renderers = new Dictionary<string, SpriteRenderer>();
        private string current;
        private Coroutine fade;

        public string Current => current;
        public IReadOnlyList<Pose> Poses => poses;

        public void Configure(List<Pose> newPoses, int sortingOrder, string initial)
        {
            poses = newPoses;
            baseSortingOrder = sortingOrder;
            initialPose = initial;
        }

        private void Awake()
        {
            foreach (var p in poses)
            {
                if (p.sprite == null || renderers.ContainsKey(p.name)) continue;
                var go = new GameObject("Pose_" + p.name);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = p.sprite;
                sr.sortingOrder = baseSortingOrder;
                Tween.SetAlpha(sr, 0f);
                renderers[p.name] = sr;
            }
            if (!string.IsNullOrEmpty(initialPose)) Snap(initialPose);
        }

        /// <summary>Instantly shows a pose.</summary>
        public void Snap(string pose)
        {
            if (!renderers.TryGetValue(pose, out var target)) return;
            if (fade != null) StopCoroutine(fade);
            foreach (var kv in renderers)
            {
                kv.Value.sortingOrder = baseSortingOrder;
                Tween.SetAlpha(kv.Value, kv.Value == target ? 1f : 0f);
            }
            current = pose;
        }

        /// <summary>Cross-fades to <paramref name="pose"/> over <paramref name="duration"/> seconds.</summary>
        public void Show(string pose, float duration = 0.15f)
        {
            if (pose == current || !renderers.TryGetValue(pose, out var target)) return;
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(CrossfadeRoutine(pose, target, duration));
        }

        private IEnumerator CrossfadeRoutine(string pose, SpriteRenderer target, float duration)
        {
            // Settle any earlier fade: everything except the previous pose is hidden.
            SpriteRenderer previous = current != null ? renderers[current] : null;
            foreach (var kv in renderers)
                if (kv.Value != previous && kv.Value != target) Tween.SetAlpha(kv.Value, 0f);

            // Cross-fades occupy only two layers. Increasing the order on every change eventually
            // put the character in front of the counter (and even in front of scene overlays).
            foreach (var kv in renderers) kv.Value.sortingOrder = baseSortingOrder;
            target.sortingOrder = baseSortingOrder + 1;
            Tween.SetAlpha(target, 0f);
            current = pose;

            yield return Tween.Run(duration, k => Tween.SetAlpha(target, k), Easing.InOutQuad);
            foreach (var kv in renderers)
                if (kv.Value != target) Tween.SetAlpha(kv.Value, 0f);
            fade = null;
        }
    }
}
