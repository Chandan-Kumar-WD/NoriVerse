using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>
    /// The tutorial hand: fades in on an ingredient, taps it, drags a faded copy over to the bowl, then fades away.
    /// (Reference video 24.4-28.9 s.)
    /// </summary>
    public class HintHand : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer hand;
        [SerializeField] private SpriteRenderer ghost;
        [SerializeField] private SpriteRenderer tapRipple;

        private Coroutine running;
        public bool IsPlaying => running != null;

        public void Configure(SpriteRenderer handSprite, SpriteRenderer ghostSprite, SpriteRenderer ripple)
        {
            hand = handSprite; ghost = ghostSprite; tapRipple = ripple;
        }

        private void Awake() => HideImmediate();

        public void Play(Ingredient target, Vector3 bowlPoint)
        {
            Stop();
            running = StartCoroutine(PlayRoutine(target, bowlPoint));
        }

        public void Stop()
        {
            if (running != null) StopCoroutine(running);
            running = null;
            HideImmediate();
        }

        private void HideImmediate()
        {
            if (hand != null) Tween.SetAlpha(hand, 0f);
            if (ghost != null) Tween.SetAlpha(ghost, 0f);
            if (tapRipple != null) Tween.SetAlpha(tapRipple, 0f);
        }

        private IEnumerator PlayRoutine(Ingredient target, Vector3 bowlPoint)
        {
            Vector3 start = target.transform.position;
            ghost.sprite = target.Sprite.sprite;
            ghost.transform.localScale = target.transform.lossyScale;
            ghost.transform.rotation = target.transform.rotation;
            ghost.sortingOrder = 150;
            hand.sortingOrder = 160;

            // hand slides in from slightly below-right and fades up on the ingredient
            Vector3 handStart = start + new Vector3(0.7f, -0.7f, 0f);
            hand.transform.position = handStart;
            yield return Tween.Run(0.4f, k =>
            {
                hand.transform.position = Vector3.Lerp(handStart, start, k);
                Tween.SetAlpha(hand, k);
            }, Easing.OutCubic);

            // tap: hand presses down and a ripple expands
            yield return Tween.Run(0.18f, k => hand.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.9f, k), Easing.OutQuad);
            if (tapRipple != null)
            {
                tapRipple.transform.position = start;
                StartCoroutine(Ripple());
            }
            ghost.transform.position = start;
            Tween.SetAlpha(ghost, 0.55f);
            yield return Tween.Wait(0.15f);

            // drag the faded copy to the bowl
            yield return Tween.Run(1.5f, k =>
            {
                Vector3 pos = Vector3.LerpUnclamped(start, bowlPoint, k);
                hand.transform.position = pos;
                ghost.transform.position = pos;
                ghost.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI) * -10f);
            }, Easing.InOutCubic);

            // hold above the bowl, tiny bob
            yield return Tween.Run(1.3f, k =>
            {
                Vector3 bob = bowlPoint + new Vector3(0f, Mathf.Sin(k * Mathf.PI * 2f) * 0.06f, 0f);
                hand.transform.position = bob;
                ghost.transform.position = bob;
                hand.transform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, Easing.OutQuad(k));
            }, Easing.Linear);

            yield return Tween.Run(0.4f, k =>
            {
                Tween.SetAlpha(hand, 1f - k);
                Tween.SetAlpha(ghost, 0.55f * (1f - k));
            }, Easing.Linear);

            running = null;
        }

        private IEnumerator Ripple()
        {
            yield return Tween.Run(0.45f, k =>
            {
                tapRipple.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.6f, Easing.OutCubic(k));
                Tween.SetAlpha(tapRipple, 0.45f * (1f - k));
            }, Easing.Linear);
        }
    }
}
