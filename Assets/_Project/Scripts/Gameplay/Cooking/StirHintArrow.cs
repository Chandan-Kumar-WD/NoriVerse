using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>
    /// A dashed arc that draws itself clockwise along the bottom of the bowl and ends in an arrowhead, then fades from the
    /// tail: the "stir in a circle" hint (reference video 41.4-44.2 s).
    /// </summary>
    public class StirHintArrow : MonoBehaviour
    {
        [SerializeField] private float radius = 2.3f;
        [SerializeField] private float startAngle = -22f;
        [SerializeField] private float endAngle = -158f;
        [SerializeField] private int dashCount = 12;
        [SerializeField] private Color color = new Color32(96, 190, 220, 255);

        private SpriteRenderer[] dashes;
        private SpriteRenderer head;
        private Coroutine running;

        public void Configure(float arcRadius, Color arrowColor)
        {
            radius = arcRadius;
            color = arrowColor;
        }

        private void Awake()
        {
            dashes = new SpriteRenderer[dashCount];
            for (int i = 0; i < dashCount; i++)
            {
                float t = (float)i / (dashCount - 1);
                float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                Vector2 p = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                // tangent of the clockwise path
                Vector2 tangent = new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle));
                var go = new GameObject("Dash" + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = p;
                go.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
                go.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSprites.Capsule();
                sr.color = new Color(color.r, color.g, color.b, 0f);
                sr.sortingOrder = 120;
                dashes[i] = sr;
            }

            float endRad = endAngle * Mathf.Deg2Rad;
            Vector2 endTangent = new Vector2(Mathf.Sin(endRad), -Mathf.Cos(endRad));
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(transform, false);
            headGo.transform.localPosition = new Vector2(Mathf.Cos(endRad), Mathf.Sin(endRad)) * radius + endTangent * 0.08f;
            headGo.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(endTangent.y, endTangent.x) * Mathf.Rad2Deg);
            headGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            head = headGo.AddComponent<SpriteRenderer>();
            head.sprite = RuntimeSprites.Chevron();
            head.color = new Color(color.r, color.g, color.b, 0f);
            head.sortingOrder = 120;
        }

        public bool IsPlaying => running != null;

        public void Play()
        {
            Stop();
            running = StartCoroutine(PlayRoutine());
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            SetAll(0f);
        }

        private void SetAll(float alpha)
        {
            if (dashes == null) return;
            foreach (var d in dashes) Tween.SetAlpha(d, alpha);
            if (head != null) Tween.SetAlpha(head, alpha);
        }

        private IEnumerator PlayRoutine()
        {
            // draw: dashes appear one after another along the path, the arrowhead last
            yield return Tween.Run(1.5f, k =>
            {
                for (int i = 0; i < dashes.Length; i++)
                {
                    float a = Mathf.Clamp01(k * (dashes.Length + 1) - i);
                    Tween.SetAlpha(dashes[i], a);
                }
                Tween.SetAlpha(head, Mathf.Clamp01(k * (dashes.Length + 1) - dashes.Length));
            }, Easing.Linear);
            yield return Tween.Wait(0.25f);
            // retract: fades from the tail so it reads as a motion
            yield return Tween.Run(1.1f, k =>
            {
                for (int i = 0; i < dashes.Length; i++)
                    Tween.SetAlpha(dashes[i], 1f - Mathf.Clamp01(k * (dashes.Length + 1) - i));
                Tween.SetAlpha(head, 1f - Mathf.Clamp01(k * 2f));
            }, Easing.Linear);
            SetAll(0f);
            running = null;
        }
    }
}
