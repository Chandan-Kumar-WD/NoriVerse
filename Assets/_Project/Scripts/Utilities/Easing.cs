using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>Easing curves used by hand-written tweens. All take and return values in 0..1.</summary>
    public static class Easing
    {
        public static float Linear(float t) => t;
        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float InOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        public static float InCubic(float t) => t * t * t;
        public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        public static float OutSine(float t) => Mathf.Sin(t * Mathf.PI * 0.5f);
        public static float InOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

        /// <summary>Overshoots past 1 and settles back, for pops and bounces.</summary>
        public static float OutBack(float t, float overshoot = 1.70158f)
        {
            float c3 = overshoot + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + overshoot * Mathf.Pow(t - 1f, 2f);
        }

        public static float SmoothStep(float edge0, float edge1, float x)
        {
            if (Mathf.Approximately(edge0, edge1)) return x >= edge1 ? 1f : 0f;
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }
}
