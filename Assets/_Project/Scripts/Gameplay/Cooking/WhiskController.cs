using System;
using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>
    /// The whisk: swings in from above, follows the child's finger inside the bowl with its handle trailing behind,
    /// and reports how far it has stirred. Any scribbling inside the bowl counts, so small hands are not penalised.
    /// </summary>
    public class WhiskController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private BowlArea bowl;
        [SerializeField] private Transform whisk;          // pivot sits on the whisk head
        [SerializeField] private SpriteRenderer whiskSprite;
        [SerializeField] private float stirDistanceNeeded = 30f;
        [SerializeField] private float headRadiusLimit = 2.6f;

        private bool enteredBowl;
        private bool active;
        private Vector3 headVelocity;
        private Vector3 lastHead;
        private float handleAngle = 8f;
        private float progress;
        private float lastCenterAngle;
        private float scaleAtRest = 1f;

        public float Progress => progress;
        public bool IsActive => active;
        public event Action<float, float> Stirred;    // (progress, swirl degrees this frame)
        public event Action StirringStarted;
        public event Action<float> ProgressChanged;

        public void Configure(Camera cam, BowlArea bowlArea, Transform whiskRoot, SpriteRenderer sprite)
        {
            sceneCamera = cam; bowl = bowlArea; whisk = whiskRoot; whiskSprite = sprite;
        }

        private void Awake()
        {
            if (whisk != null) scaleAtRest = whisk.localScale.x;
            whiskSprite.enabled = false;
        }

        /// <summary>Slides in from the top of the screen and settles with the handle swinging to the right (video 40.5-41.3 s).</summary>
        public IEnumerator Enter(Vector3 restPosition)
        {
            whiskSprite.enabled = true;
            AudioDirector.Play(SoundId.WhiskFlyIn);
            Vector3 from = restPosition + new Vector3(0.9f, 6.5f, 0f);
            whisk.position = from;
            whisk.rotation = Quaternion.Euler(0f, 0f, 62f);
            whisk.localScale = Vector3.one * scaleAtRest * 1.15f;
            yield return Tween.Run(0.7f, k =>
            {
                whisk.position = Vector3.LerpUnclamped(from, restPosition, k);
                whisk.localScale = Vector3.one * Mathf.Lerp(scaleAtRest * 1.15f, scaleAtRest, k);
            }, t => Easing.OutBack(t, 0.9f));
            yield return Tween.Run(0.45f, k => whisk.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(62f, 8f, k)), t => Easing.OutBack(t, 1.4f));
            handleAngle = 8f;
            lastHead = whisk.position;
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            PointerState p = PointerInput.Read(sceneCamera);
            if (!p.IsDown) return;

            Vector2 center = bowl.Center;
            Vector2 target = p.world;
            Vector2 offset = target - center;
            if (offset.magnitude > headRadiusLimit) target = center + offset.normalized * headRadiusLimit;

            // first touch must be near the whisk to pick it up
            if (p.phase == PointerPhase.Began)
            {
                enteredBowl = Vector2.Distance(p.world, whisk.position) < 2.6f;
                headVelocity = Vector3.zero;
                lastHead = whisk.position;
            }
            if (!enteredBowl) return;

            Vector3 head = Vector3.SmoothDamp(whisk.position, new Vector3(target.x, target.y, whisk.position.z), ref headVelocity, 0.05f);
            Vector3 delta = head - lastHead;
            float step = delta.magnitude;
            whisk.position = head;

            if (step > 0.02f)
            {
                // handle trails behind the head: it points opposite to the direction of travel, with some lag
                float desired = Mathf.Atan2(-delta.y, -delta.x) * Mathf.Rad2Deg;
                handleAngle = Mathf.LerpAngle(handleAngle, desired, 9f * Time.deltaTime);
                whisk.rotation = Quaternion.Euler(0f, 0f, handleAngle);
            }

            if (step > 0.001f)
            {
                if (progress <= 0f) StirringStarted?.Invoke();
                if (step > 0.01f && progress < 1f) AudioDirector.Sustain(SoundId.WhiskStirLoop);
                float before = progress;
                progress = Mathf.Min(1f, progress + step / stirDistanceNeeded);
                // swirl direction follows the finger around the bowl centre
                float angle = Mathf.Atan2(head.y - center.y, head.x - center.x) * Mathf.Rad2Deg;
                float swirl = Mathf.DeltaAngle(lastCenterAngle, angle);
                lastCenterAngle = angle;
                if (p.phase == PointerPhase.Began) swirl = 0f;
                Stirred?.Invoke(progress, Mathf.Clamp(swirl, -25f, 25f));
                if (!Mathf.Approximately(before, progress)) ProgressChanged?.Invoke(progress);
            }
            lastHead = head;

            // the whisk sinks into the batter as it thickens
            float sink = Mathf.Lerp(1f, 0.78f, Easing.SmoothStep(0f, 0.4f, progress));
            whisk.localScale = Vector3.one * scaleAtRest * sink;
        }

        public void Deactivate() => active = false;

        /// <summary>Whisk lifts out of the bowl and fades when the batter is done.</summary>
        public IEnumerator Leave()
        {
            active = false;
            Vector3 from = whisk.position;
            Vector3 to = from + new Vector3(1.2f, 2.6f, 0f);
            yield return Tween.Run(0.6f, k =>
            {
                whisk.position = Vector3.LerpUnclamped(from, to, k);
                Tween.SetAlpha(whiskSprite, 1f - k);
            }, Easing.InCubic);
            whiskSprite.enabled = false;
        }
    }
}
