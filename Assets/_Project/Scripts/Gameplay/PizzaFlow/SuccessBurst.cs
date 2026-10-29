using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class SuccessBurst : MonoBehaviour
    {
        private SpriteRenderer[] particles;
        private Coroutine routine;
        [SerializeField] private int sortingOrder=100;
        public void ConfigureSorting(int order)=>sortingOrder=order;
        public void Play(Vector2 center, Color color, float radius = 3.4f)
        {
            if (particles == null)
            {
                particles = new SpriteRenderer[16];
                for (int i = 0; i < particles.Length; i++)
                {
                    var go = new GameObject("Sparkle"); go.transform.SetParent(transform, false);
                    particles[i] = go.AddComponent<SpriteRenderer>();
                    particles[i].sprite = RuntimeSprites.Sparkle(); particles[i].sortingOrder = sortingOrder;
                    Tween.SetAlpha(particles[i], 0);
                }
            }
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Animate(center, color, radius));
        }
        private IEnumerator Animate(Vector2 center, Color color, float radius)
        {
            yield return Tween.Run(0.8f, k =>
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    float angle = i * Mathf.PI * 2f / particles.Length;
                    Vector2 d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    particles[i].transform.position = center + d * (radius + k * 0.7f);
                    particles[i].transform.localScale = Vector3.one * (Mathf.Sin(k * Mathf.PI) * (i % 2 == 0 ? 0.22f : 0.13f));
                    particles[i].transform.rotation = Quaternion.Euler(0, 0, k * 160f + i * 37f);
                    particles[i].color = new Color(color.r, color.g, color.b, 1f - k);
                }
            }, Easing.OutQuad);
            foreach (var p in particles) Tween.SetAlpha(p, 0);
            routine = null;
        }
    }
}
