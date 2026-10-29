using System.Collections;
using NoriVerse.Characters;
using NoriVerse.Core;
using NoriVerse.Utilities;
using NoriVerse.Gameplay.PizzaFinish;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.Kitchen
{
    /// <summary>
    /// The kitchen menu scene: the pot from the end of the splash wobbles on white, the kitchen fades in, Nori idles, and
    /// tapping the first (unlocked) dish starts the cooking screen. Locked dishes wobble.
    /// </summary>
    public class KitchenMenuController : MonoBehaviour
    {
        [Header("Intro")]
        [SerializeField] private SpriteRenderer whiteOverlay;
        [SerializeField] private Transform pot;
        [SerializeField] private Transform potLid;
        [SerializeField] private SpriteRenderer[] potSprites;

        [Header("Menu")]
        [SerializeField] private NoriMoodDirector nori;
        [SerializeField] private DishButton[] dishes;
        [SerializeField] private string nextSceneName = "Cooking";
        [SerializeField] private Camera sceneCamera;

        private bool interactive;
        private bool chosen;
        [SerializeField] private PizzaServingController serving;
        public bool IsInteractive=>interactive&&!chosen;
        public void AttachServing(PizzaServingController value)=>serving=value;

        public void Configure(SpriteRenderer overlay, Transform potRoot, Transform lid, SpriteRenderer[] potRenderers,
            NoriMoodDirector mood, DishButton[] dishButtons, Camera cam)
        {
            whiteOverlay = overlay; pot = potRoot; potLid = lid; potSprites = potRenderers;
            nori = mood; dishes = dishButtons; sceneCamera = cam;
        }

        private void Awake()=>PizzaRecipeState.RestoreCompletedForHome();

        private IEnumerator Start()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            AudioDirector.Music(SoundId.MusGameplayLoop);
            if(PizzaRecipeState.IsBaked)
            {pot.gameObject.SetActive(false);Tween.SetAlpha(whiteOverlay,0);interactive=true;nori.BeginLoop();yield break;}
            Tween.SetAlpha(whiteOverlay, 1f);

            // 0.00-0.90s: the pot sits on white with its lid rocking, then fades away.
            Vector3 potRest = pot.localPosition;
            for (float t = 0f; t < 0.85f; t += Time.deltaTime)
            {
                potLid.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 11f) * 3.5f);
                pot.localPosition = potRest + new Vector3(0f, Mathf.Sin(t * 7f) * 0.04f, 0f);
                yield return null;
            }
            yield return Tween.Run(0.15f, k =>
            {
                foreach (var sr in potSprites) Tween.SetAlpha(sr, 1f - k);
                pot.localScale = Vector3.one * Mathf.Lerp(1f, 0.93f, k);
            }, Easing.InQuad);
            pot.gameObject.SetActive(false);

            // 1.55-1.95s: the kitchen fades in from white.
            yield return Tween.Wait(0.55f);
            yield return Tween.Fade(whiteOverlay, 0f, 0.4f, Easing.InOutQuad);

            interactive = true;
            nori.BeginLoop();
        }

        private void Update()
        {
            if (!interactive || chosen || SceneTransition.IsBusy) return;
            PointerState p = PointerInput.Read(sceneCamera);
            if(serving!=null && serving.IsBusy)return;
            if(p.IsDown)nori.NotifyActivity();
            if (p.phase != PointerPhase.Began) return;
            if(serving!=null && serving.TryBeginDrag(p.world))return;

            Collider2D hit = Physics2D.OverlapPoint(p.world);
            if (hit == null) return;
            var dish = hit.GetComponentInParent<DishButton>();
            if (dish == null) return;

            TapDish(dish);
        }

        /// <summary>A dish was tapped: locked ones wobble, the unlocked one starts cooking.</summary>
        public void TapDish(DishButton dish)
        {
            if (!interactive || chosen) return;
            if (dish.IsLocked) { AudioDirector.Play(SoundId.DishLockedWobble); dish.Wobble(); }
            else StartCoroutine(Choose(dish));
        }

        private IEnumerator Choose(DishButton dish)
        {
            serving?.CancelForReplay();PizzaRecipeState.BeginNewRecipe();
            chosen = true;
            AudioDirector.Play(SoundId.UiTap);
            AudioDirector.Voice(SoundId.VoOoh);
            nori.Cheer();
            yield return dish.PopRoutine();
            yield return Tween.Wait(0.2f);
            SceneTransition.Go(nextSceneName);
        }
    }
}
