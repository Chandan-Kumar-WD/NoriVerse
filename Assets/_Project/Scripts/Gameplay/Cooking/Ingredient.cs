using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.Cooking
{
    public enum IngredientKind { Butter, Flour, Sugar }
    public enum IngredientState { Hidden, InTray, Dragging, InBowl }

    /// <summary>
    /// One ingredient on the tray. Owns its own animations (pop in, lift, settle in the bowl, dissolve while mixing);
    /// <see cref="IngredientDragController"/> decides when they run.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Ingredient : MonoBehaviour
    {
        [SerializeField] private IngredientKind kind;
        [SerializeField] private float landedScale = 1f;

        private SpriteRenderer sprite;
        private Vector3 homePosition;
        private float homeRotation;
        private Vector3 homeScale = Vector3.one;
        private Coroutine anim;

        public IngredientKind Kind => kind;
        public IngredientState State { get; private set; } = IngredientState.Hidden;
        public SpriteRenderer Sprite => sprite;
        public Vector3 HomePosition => homePosition;
        public float RestScale => homeScale.x;

        public void Configure(IngredientKind ingredientKind, float landingScale)
        {
            kind = ingredientKind;
            landedScale = landingScale;
        }

        private void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            homePosition = transform.localPosition;
            homeRotation = transform.localEulerAngles.z;
            homeScale = transform.localScale;
            transform.localScale = Vector3.zero;
            GetComponent<Collider2D>().enabled = false;
        }

        /// <summary>Pops into the tray: 0 -> 1.15 -> 1 with a little overshoot.</summary>
        public void PopIn(float delay)
        {
            Run(PopRoutine(delay));
        }

        private IEnumerator PopRoutine(float delay)
        {
            yield return Tween.Wait(delay);
            State = IngredientState.InTray;
            GetComponent<Collider2D>().enabled = true;
            AudioDirector.Play(SoundId.ItemPopIn, Random.Range(0.92f, 1.12f), 0.03f);
            yield return Tween.Run(0.3f, k => transform.localScale = homeScale * Mathf.LerpUnclamped(0f, 1f, Easing.OutBack(k, 2.4f)), null);
            transform.localScale = homeScale;
        }

        /// <summary>Lifts off the tray when picked up.</summary>
        public void BeginDrag(int sortingOrder)
        {
            State = IngredientState.Dragging;
            sprite.sortingOrder = sortingOrder;
            Run(Tween.Run(0.12f, k => transform.localScale = homeScale * Mathf.Lerp(1f, 1.12f, k), Easing.OutQuad));
        }

        /// <summary>Slides back to its spot in the tray when released outside the bowl.</summary>
        public void ReturnHome(int sortingOrder)
        {
            State = IngredientState.InTray;
            sprite.sortingOrder = sortingOrder;
            Run(ReturnRoutine());
        }

        private IEnumerator ReturnRoutine()
        {
            Vector3 from = transform.position;
            Quaternion fromRot = transform.rotation;
            Vector3 fromScale = transform.localScale;
            Vector3 to = transform.parent != null ? transform.parent.TransformPoint(homePosition) : homePosition;
            yield return Tween.Run(0.35f, k =>
            {
                transform.position = Vector3.LerpUnclamped(from, to, k);
                transform.rotation = Quaternion.SlerpUnclamped(fromRot, Quaternion.Euler(0f, 0f, homeRotation), k);
                transform.localScale = Vector3.LerpUnclamped(fromScale, homeScale, k);
            }, t => Easing.OutBack(t, 1.2f));
        }

        /// <summary>Drops into the bowl at <paramref name="target"/> (world) and settles.</summary>
        public void Land(Vector3 target, float rotationZ, int sortingOrder)
        {
            State = IngredientState.InBowl;
            sprite.sortingOrder = sortingOrder;
            GetComponent<Collider2D>().enabled = false;
            Run(LandRoutine(target, rotationZ));
        }

        private IEnumerator LandRoutine(Vector3 target, float rotationZ)
        {
            Vector3 from = transform.position;
            Quaternion fromRot = transform.rotation;
            Vector3 fromScale = transform.localScale;
            Vector3 toScale = homeScale * landedScale;
            yield return Tween.Run(0.22f, k =>
            {
                transform.position = Vector3.LerpUnclamped(from, target, k);
                transform.rotation = Quaternion.SlerpUnclamped(fromRot, Quaternion.Euler(0f, 0f, rotationZ), k);
            }, Easing.OutCubic);
            // plop: squash then settle (flour spreads into a bigger pile)
            yield return Tween.Run(0.14f, k => transform.localScale = Vector3.LerpUnclamped(fromScale, toScale * 1.12f, k), Easing.OutQuad);
            yield return Tween.Run(0.22f, k => transform.localScale = Vector3.LerpUnclamped(toScale * 1.12f, toScale, k), Easing.InOutCubic);
        }

        /// <summary>Called while mixing: fades the ingredient out and swirls it around the bowl centre.</summary>
        public void SetDissolve(float amount, Vector3 bowlCenter, float swirlDegrees)
        {
            if (State != IngredientState.InBowl) return;
            Color c = sprite.color;
            c.a = 1f - amount;
            sprite.color = c;
            transform.RotateAround(bowlCenter, Vector3.forward, swirlDegrees);
        }

        private void Run(IEnumerator routine)
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(routine);
        }
    }
}
