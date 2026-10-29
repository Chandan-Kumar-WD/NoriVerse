using System.Collections;
using NoriVerse.Characters;
using NoriVerse.Core;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEngine;
using UnityEngine.Events;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public class PizzaServingController:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private KitchenMenuController menu;
        [SerializeField] private DishButton sourceDish;
        [SerializeField] private SpriteRenderer meal;
        [SerializeField] private PizzaVisual snapshotSource;
        [SerializeField] private NoriMoodDirector mood;
        [SerializeField] private SpriteCrossfader face;
        [SerializeField] private Transform neck;
        [SerializeField] private GestureHint hint;
        [SerializeField] private MealReactionFx reactions;
        [SerializeField] private UnityEvent onPizzaServed=new UnityEvent();
        private Sprite snapshot;
        private MealBiteVisual bites;
        private NoriEatingAnimator eatingAnimation;
        public int BitesTaken=>bites!=null?bites.BitesTaken:0;
        public string EatingStage=>eatingAnimation!=null?eatingAnimation.Stage:"Idle";
        private Vector2 offset,pressPoint;
        private Vector3 rest;
        private bool dragging,eating,returning;
        private bool completionCue;
        private float completionCueAt;
        private Vector2[] feedingPath;
        public bool IsFeedingGuidePlaying=>hint.IsPlaying;
        public bool IsBusy=>dragging||eating||returning;
        public int Servings=>PizzaRecipeState.Servings;
        public Sprite ServedSprite=>snapshot;
        public Vector2 MouthPosition=>neck.TransformPoint(new Vector3(0,0.85f,0));
        public UnityEvent OnPizzaServed=>onPizzaServed;
        public void Configure(Camera cam,KitchenMenuController controller,DishButton dish,SpriteRenderer held,PizzaVisual source,NoriMoodDirector director,SpriteCrossfader expressions,Transform pivot,GestureHint gesture,MealReactionFx fx)
        {sceneCamera=cam;menu=controller;sourceDish=dish;meal=held;snapshotSource=source;mood=director;face=expressions;neck=pivot;hint=gesture;reactions=fx;}
        private void Awake()
        {
            rest=neck.localPosition;Tween.SetAlpha(meal,0);
            eatingAnimation=gameObject.AddComponent<NoriEatingAnimator>();eatingAnimation.Configure(face);
        }
        private IEnumerator Start()
        {
            if(!PizzaRecipeState.IsBaked)yield break;
            while(!menu.IsInteractive || SceneTransition.IsBusy)yield return null;
            EnsureSnapshot();hint.SetDelay(12f);PizzaRecipeState.HasReturnedToKitchen=true;
            completionCue=PizzaRecipeState.ConsumeCompletionHint();completionCueAt=Time.time+1f;
            feedingPath=new[]{(Vector2)sourceDish.transform.position,MouthPosition};
        }
        private void EnsureSnapshot()
        {
            if(snapshot!=null)return;
            snapshot=PizzaSnapshot.Create(snapshotSource);meal.sprite=snapshot;
            bites=meal.gameObject.AddComponent<MealBiteVisual>();bites.Configure(meal,snapshot);
        }
        public bool TryBeginDrag(Vector2 world)
        {
            if(!PizzaRecipeState.IsBaked || IsBusy || !sourceDish.GetComponent<Collider2D>().OverlapPoint(world))return false;
            EnsureSnapshot();bites.ResetMeal();AudioDirector.Play(SoundId.ItemPickup);dragging=true;pressPoint=world;offset=(Vector2)sourceDish.transform.position-world;
            meal.transform.position=sourceDish.transform.position;meal.transform.localScale=Vector3.one;Tween.SetAlpha(meal,1);
            hint.Stop();reactions.Stop();mood.Stop();eatingAnimation.Anticipate(false);return true;
        }
        private void Update()
        {
            if(!PizzaRecipeState.IsBaked || !menu.IsInteractive || SceneTransition.IsBusy)return;
            var p=PointerInput.Read(sceneCamera);
            if(p.IsDown)
            {
                completionCue=false;hint.ResetIdle();
            }
            if(dragging && p.IsDown)
            {
                meal.transform.position=p.world+offset;
                neck.localRotation=Quaternion.Euler(0,0,Mathf.Clamp(-p.world.x*.3f,-1.2f,1.2f));
                eatingAnimation.Anticipate(Vector2.Distance(meal.transform.position,MouthPosition)<2.2f);
            }
            if(dragging && p.phase==PointerPhase.Ended)
            {
                dragging=false;
                if(Vector2.Distance(pressPoint,p.world)<0.25f)
                {Tween.SetAlpha(meal,0);menu.TapDish(sourceDish);return;}
                Vector2 target=p.world-(Vector2)MouthPosition;
                if(Mathf.Abs(target.x)<1.8f && Mathf.Abs(target.y)<1.25f)StartCoroutine(Eat());
                else StartCoroutine(Return());
            }
            if(!IsBusy&&snapshot!=null&&feedingPath!=null)
            {
                feedingPath[0]=sourceDish.transform.position;feedingPath[1]=MouthPosition;
                if(completionCue&&Time.time>=completionCueAt)
                {completionCue=false;hint.PlayOnce(feedingPath,snapshot,Vector3.one);}
                else hint.Offer(feedingPath,snapshot,Vector3.one);
            }
        }
        public void CancelForReplay(){dragging=eating=returning=false;StopAllCoroutines();hint.Stop();reactions.Stop();Tween.SetAlpha(meal,0);ResetHead();}
        private void ResetHead(){neck.localPosition=rest;neck.localRotation=Quaternion.identity;neck.localScale=Vector3.one;eatingAnimation.ResetPose();}
        private IEnumerator Return()
        {
            returning=true;AudioDirector.Play(SoundId.DropReturn);yield return Tween.Move(meal.transform,sourceDish.transform.position,0.4f,Easing.OutCubic);
            Tween.SetAlpha(meal,0);ResetHead();mood.BeginLoop();hint.ResetIdle();returning=false;
        }
        private IEnumerator Eat()
        {
            eating=true;ResetHead();eatingAnimation.Anticipate(true);
            Vector3 from=meal.transform.position;Vector3 mouth=(Vector3)MouthPosition+new Vector3(0,-.30f,0);
            yield return Tween.Run(.32f,k=>
            {
                meal.transform.position=Vector3.Lerp(from,mouth,k)+new Vector3(0,Mathf.Sin(k*Mathf.PI)*.12f,0);
                meal.transform.localScale=Vector3.one*Mathf.Lerp(1,.52f,k);
                meal.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(k*Mathf.PI)*-5f);
            },Easing.InOutCubic);
            yield return eatingAnimation.Eat((index,k)=>
            {
                bites.Bite(index,k);
                meal.transform.localScale=Vector3.one*Mathf.Lerp(.52f,.40f,(index+k)/3f);
                if(index==2)Tween.SetAlpha(meal,1-k);
            },index=>reactions.PlayCrumbs(mouth+new Vector3(0,.3f,0),index));
            Tween.SetAlpha(meal,0);meal.transform.localRotation=Quaternion.identity;
            PizzaRecipeState.Servings++;onPizzaServed.Invoke();reactions.Play(PizzaRecipeState.Servings,new Vector2(0,1.6f));
            AudioDirector.Play(SoundId.CutCompleteSparkle);
            yield return Tween.Wait(0.35f);ResetHead();mood.BeginLoop();hint.ResetIdle();eating=false;
        }
        private void OnDestroy(){if(snapshot!=null){Destroy(snapshot.texture);Destroy(snapshot);}}
    }
}
