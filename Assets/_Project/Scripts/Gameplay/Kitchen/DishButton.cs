using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.Kitchen
{
    /// <summary>One recipe on the counter. Unlocked dishes can be chosen; locked ones wobble when tapped.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class DishButton : MonoBehaviour
    {
        [SerializeField] private bool locked = true;
        [SerializeField] private Transform visual;
        [SerializeField] private GameObject lockIcon;

        private Coroutine running;
        private Vector3 restScale = Vector3.one;

        public bool IsLocked => locked;

        public void Configure(bool isLocked, Transform visualRoot, GameObject padlock)
        {
            locked = isLocked;
            visual = visualRoot;
            lockIcon = padlock;
        }

        private void Awake()
        {
            if (visual == null) visual = transform;
            restScale = visual.localScale;
            if (lockIcon != null) lockIcon.SetActive(locked);
        }

        /// <summary>Squish-and-pop when the dish is chosen.</summary>
        public IEnumerator PopRoutine()
        {
            yield return Tween.Run(0.12f, k => visual.localScale = restScale * Mathf.Lerp(1f, 0.9f, k), Easing.OutQuad);
            yield return Tween.Run(0.28f, k => visual.localScale = restScale * Mathf.LerpUnclamped(0.9f, 1f, Easing.OutBack(k, 3f)), null);
        }

        /// <summary>Shake side to side to say "not available yet".</summary>
        public void Wobble()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(WobbleRoutine());
        }

        private IEnumerator WobbleRoutine()
        {
            yield return Tween.Run(0.55f, k =>
            {
                float damp = 1f - k;
                visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 6f) * 9f * damp);
            }, Easing.Linear);
            visual.localRotation = Quaternion.identity;
            running = null;
        }
    }
}
