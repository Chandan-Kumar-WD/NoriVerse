using System;
using System.Collections.Generic;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>Lets the child drag ingredients from the tray into the bowl (mouse or touch).</summary>
    public class IngredientDragController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private BowlArea bowl;
        [SerializeField] private Ingredient[] ingredients;
        [SerializeField] private SpriteRenderer touchDot;

        private Ingredient dragged;
        private Vector3 smoothVelocity;
        private Vector3 lastPosition;
        private float tilt;
        private int landedCount;
        private int nextBowlOrder = 40;

        public event Action<Ingredient> IngredientAdded;
        public event Action AllIngredientsAdded;
        public event Action PointerTouched;

        public int RemainingCount => ingredients.Length - landedCount;
        public IReadOnlyList<Ingredient> Ingredients => ingredients;

        public void Configure(Camera cam, BowlArea bowlArea, Ingredient[] items, SpriteRenderer dot)
        {
            sceneCamera = cam; bowl = bowlArea; ingredients = items; touchDot = dot;
        }

        private void Awake()
        {
            if (touchDot != null) Tween.SetAlpha(touchDot, 0f);
        }

        private void Update()
        {
            PointerState p = PointerInput.Read(sceneCamera);
            if (p.phase == PointerPhase.Began) PointerTouched?.Invoke();
            if (landedCount >= ingredients.Length) return;

            switch (p.phase)
            {
                case PointerPhase.Began: TryPick(p.world); break;
                case PointerPhase.Held: if (dragged != null) Drag(p.world); break;
                case PointerPhase.Ended: if (dragged != null) Release(p.world); break;
            }
        }

        private void TryPick(Vector2 world)
        {
            Ingredient best = null;
            foreach (var hit in Physics2D.OverlapPointAll(world))
            {
                var ing = hit.GetComponent<Ingredient>();
                if (ing != null && ing.State == IngredientState.InTray && (best == null || ing.Sprite.sortingOrder > best.Sprite.sortingOrder))
                    best = ing;
            }
            if (best == null) return;

            dragged = best;
            AudioDirector.Play(SoundId.ItemPickup);
            dragged.BeginDrag(200);
            smoothVelocity = Vector3.zero;
            lastPosition = dragged.transform.position;
            if (touchDot != null) Tween.SetAlpha(touchDot, 0.55f);
        }

        private void Drag(Vector2 world)
        {
            Vector3 target = new Vector3(world.x, world.y, dragged.transform.position.z);
            Vector3 pos = Vector3.SmoothDamp(dragged.transform.position, target, ref smoothVelocity, 0.04f);
            dragged.transform.position = pos;

            // lean into the direction of travel, like something carried in a hand
            float sideSpeed = (pos.x - lastPosition.x) / Mathf.Max(Time.deltaTime, 0.0001f);
            tilt = Mathf.Lerp(tilt, Mathf.Clamp(-sideSpeed * 1.4f, -18f, 18f), 12f * Time.deltaTime);
            dragged.transform.rotation = Quaternion.Euler(0f, 0f, tilt);
            lastPosition = pos;

            if (touchDot != null) touchDot.transform.position = new Vector3(world.x, world.y, touchDot.transform.position.z);
        }

        private void Release(Vector2 world)
        {
            if (touchDot != null) Tween.SetAlpha(touchDot, 0f);
            var item = dragged;
            dragged = null;
            tilt = 0f;

            if (bowl.Contains(item.transform.position, 0.2f))
            {
                Vector2 spot = bowl.ClampInside(item.transform.position, 1.0f);
                item.Land(new Vector3(spot.x, spot.y, item.transform.position.z), UnityEngine.Random.Range(-35f, 35f), nextBowlOrder++);
                landedCount++;
                AudioDirector.Play(SoundId.DropPlopBowl, UnityEngine.Random.Range(0.94f, 1.08f));
                IngredientAdded?.Invoke(item);
                if (landedCount >= ingredients.Length) AllIngredientsAdded?.Invoke();
            }
            else
            {
                AudioDirector.Play(SoundId.DropReturn);
                item.ReturnHome(20);
            }
        }

        /// <summary>Cancels a drag (e.g. when the scene moves on).</summary>
        public void CancelDrag()
        {
            if (dragged == null) return;
            dragged.ReturnHome(20);
            dragged = null;
            if (touchDot != null) Tween.SetAlpha(touchDot, 0f);
        }
    }
}
