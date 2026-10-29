using System.Collections;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using UnityEngine.Events;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>
    /// The "mix the batter" screen: ingredients pop into the tray, the child drags them into the bowl (a hand shows how if
    /// they wait), a whisk appears, and stirring turns everything into pancake batter. Timings follow the reference video.
    /// </summary>
    public class CookingController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer whiteOverlay;
        [SerializeField] private BowlArea bowl;

        [Header("Parts")]
        [SerializeField] private IngredientDragController dragController;
        [SerializeField] private HintHand hintHand;
        [SerializeField] private WhiskController whisk;
        [SerializeField] private StirHintArrow stirArrow;
        [SerializeField] private BatterVisual batter;
        [SerializeField] private Vector2 whiskRestOffset = new Vector2(1.45f, 0.55f);

        [Header("Flow")]
        [Tooltip("Scene to load once the batter is ready.")]
        [SerializeField] private string nextSceneName = "Dough";
        [SerializeField] private float[] popInDelays = { 0.10f, 0.25f, 0.20f, 0.38f, 0.60f, 0.65f };
        [SerializeField] private float firstHintAfter = 6.0f;
        [SerializeField] private float repeatHintAfter = 5.5f;
        [SerializeField] private UnityEvent onBatterReady = new UnityEvent();

        private float lastTouchTime;
        private bool whiskPhase;
        private bool done;

        public UnityEvent OnBatterReady => onBatterReady;
        public void SetNextScene(string sceneName) => nextSceneName = sceneName;

        public void Configure(Camera cam, SpriteRenderer overlay, BowlArea bowlArea, IngredientDragController drag, HintHand hand,
            WhiskController whiskController, StirHintArrow arrow, BatterVisual batterVisual)
        {
            sceneCamera = cam; whiteOverlay = overlay; bowl = bowlArea; dragController = drag; hintHand = hand;
            whisk = whiskController; stirArrow = arrow; batter = batterVisual;
        }

        private IEnumerator Start()
        {
            if (sceneCamera == null) sceneCamera = Camera.main;
            AudioDirector.Music(SoundId.MusGameplayLoop);
            lastTouchTime = Time.time;

            dragController.IngredientAdded += OnIngredientAdded;
            dragController.AllIngredientsAdded += OnAllIngredientsAdded;
            whisk.Stirred += OnStirred;
            whisk.StirringStarted += OnStirringStarted;

            // Arriving through SceneTransition the white fade-in is handled for us; when the scene is run on its own we do it.
            if (!SceneTransition.IsBusy && whiteOverlay != null)
            {
                Tween.SetAlpha(whiteOverlay, 1f);
                yield return Tween.Fade(whiteOverlay, 0f, 0.3f, Easing.InOutQuad);
            }
            else if (whiteOverlay != null) Tween.SetAlpha(whiteOverlay, 0f);

            for (int i = 0; i < dragController.Ingredients.Count; i++)
                dragController.Ingredients[i].PopIn(popInDelays[Mathf.Min(i, popInDelays.Length - 1)]);

            StartCoroutine(IngredientHintLoop());
        }

        private void Update()
        {
            if (PointerInput.Read(sceneCamera).phase == PointerPhase.Began)
            {
                lastTouchTime = Time.time;
                if (hintHand.IsPlaying) hintHand.Stop();
            }
        }

        // ---- hints -------------------------------------------------------------------------------------------
        private IEnumerator IngredientHintLoop()
        {
            yield return Tween.Wait(firstHintAfter);
            while (!whiskPhase && !done)
            {
                if (Time.time - lastTouchTime >= repeatHintAfter - 0.01f || Time.time - lastTouchTime >= firstHintAfter - 0.01f)
                {
                    Ingredient target = NearestRemaining();
                    if (target != null)
                    {
                        hintHand.Play(target, bowl.transform.position + new Vector3(-0.4f, -0.3f, 0f));
                        while (hintHand.IsPlaying) yield return null;
                        lastTouchTime = Mathf.Max(lastTouchTime, Time.time - repeatHintAfter + 1.5f);
                    }
                }
                yield return Tween.Wait(0.5f);
            }
        }

        private Ingredient NearestRemaining()
        {
            Ingredient best = null;
            float bestDist = float.MaxValue;
            foreach (var ing in dragController.Ingredients)
            {
                if (ing.State != IngredientState.InTray) continue;
                float d = Vector2.Distance(ing.transform.position, bowl.Center);
                if (d < bestDist) { bestDist = d; best = ing; }
            }
            return best;
        }

        private IEnumerator StirHintLoop()
        {
            yield return Tween.Wait(0.2f);
            while (!done && whisk.Progress <= 0f)
            {
                stirArrow.Play();
                while (stirArrow.IsPlaying) yield return null;
                yield return Tween.Wait(6.5f);
            }
        }

        // ---- flow ----------------------------------------------------------------------------------------------
        private void OnIngredientAdded(Ingredient ingredient)
        {
            hintHand.Stop();
            batter.RegisterIngredient(ingredient);
        }

        private void OnAllIngredientsAdded()
        {
            whiskPhase = true;
            StartCoroutine(WhiskPhase());
        }

        private IEnumerator WhiskPhase()
        {
            hintHand.Stop();
            yield return Tween.Wait(0.6f);   // tray empties, then the whisk arrives (video 40.0 -> 40.5 s)
            Vector3 rest = bowl.transform.position + new Vector3(whiskRestOffset.x, whiskRestOffset.y, 0f);
            yield return whisk.Enter(rest);
            StartCoroutine(StirHintLoop());
        }

        private void OnStirringStarted() => stirArrow.Stop();

        private void OnStirred(float progress, float swirl)
        {
            batter.Apply(progress, swirl);
            if (progress >= 1f && !done) StartCoroutine(Finish());
        }

        private IEnumerator Finish()
        {
            done = true;
            AudioDirector.Play(SoundId.SuccessArpeggio);
            AudioDirector.Voice(SoundId.VoYay);
            yield return Tween.Wait(1.0f);
            yield return whisk.Leave();
            yield return Tween.Wait(0.4f);
            onBatterReady.Invoke();
            SceneTransition.Go(string.IsNullOrEmpty(nextSceneName) ? null : nextSceneName);
        }
    }
}
