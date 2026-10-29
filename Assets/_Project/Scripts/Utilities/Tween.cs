using System;
using System.Collections;
using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>Tiny coroutine tweens so scenes need no animation package. Run with StartCoroutine.</summary>
    public static class Tween
    {
        /// <summary>Calls <paramref name="apply"/> with an eased 0..1 value over <paramref name="duration"/> seconds.</summary>
        public static IEnumerator Run(float duration, Action<float> apply, Func<float, float> ease = null)
        {
            ease ??= Easing.InOutCubic;
            if (duration <= 0f) { apply(ease(1f)); yield break; }
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                apply(ease(t / duration));
                yield return null;
            }
            apply(ease(1f));
        }

        public static IEnumerator Move(Transform target, Vector3 to, float duration, Func<float, float> ease = null)
        {
            Vector3 from = target.localPosition;
            return Run(duration, k => { if (target) target.localPosition = Vector3.LerpUnclamped(from, to, k); }, ease);
        }

        public static IEnumerator Scale(Transform target, Vector3 to, float duration, Func<float, float> ease = null)
        {
            Vector3 from = target.localScale;
            return Run(duration, k => { if (target) target.localScale = Vector3.LerpUnclamped(from, to, k); }, ease);
        }

        public static IEnumerator Rotate(Transform target, float toZ, float duration, Func<float, float> ease = null)
        {
            float from = Mathf.DeltaAngle(0f, target.localEulerAngles.z);
            return Run(duration, k => { if (target) target.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(from, toZ, k)); }, ease);
        }

        public static IEnumerator Fade(SpriteRenderer target, float to, float duration, Func<float, float> ease = null)
        {
            float from = target.color.a;
            return Run(duration, k =>
            {
                if (!target) return;
                Color c = target.color;
                c.a = Mathf.LerpUnclamped(from, to, k);
                target.color = c;
            }, ease ?? Easing.Linear);
        }

        public static void SetAlpha(SpriteRenderer target, float alpha)
        {
            Color c = target.color;
            c.a = alpha;
            target.color = c;
        }

        public static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime) yield return null;
        }
    }
}
