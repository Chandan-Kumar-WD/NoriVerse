using System.Collections;
using NoriVerse.Core;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEngine;
using UnityEngine.Events;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public enum BakingPhase{Loading,Waiting,Baking,Reward,Complete}
    public class PizzaBakingController:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Transform carriage,power,flame;
        [SerializeField] private SpriteRenderer overlay,progressFill;
        [SerializeField] private PizzaVisual pizza;
        [SerializeField] private PizzaRewardController reward;
        [SerializeField] private GestureHint hint;
        private BakingProgressSparkles progressSparkles;
        [SerializeField] private UnityEvent onDishCompleted=new UnityEvent();
        private Vector3 fillScale,fillPosition;
        private float fillWidth;
        public BakingPhase Phase{get;private set;}
        public float Progress{get;private set;}
        public Vector2 PowerPosition=>power.position;
        public UnityEvent OnDishCompleted=>onDishCompleted;
        public void Configure(Camera cam,Transform rack,Transform button,Transform heat,SpriteRenderer cover,SpriteRenderer bar,PizzaVisual meal,PizzaRewardController awards,GestureHint gesture)
        {sceneCamera=cam;carriage=rack;power=button;flame=heat;overlay=cover;progressFill=bar;pizza=meal;reward=awards;hint=gesture;}
        private IEnumerator Start()
        {
            AudioDirector.Music(SoundId.MusGameplayLoop);
            PizzaRecipeState.EnsurePreparedForDirectEntry();pizza.Rebuild();pizza.SetBakedAmount(0);
            progressSparkles=gameObject.AddComponent<BakingProgressSparkles>();progressSparkles.Configure(progressFill);
            fillScale=progressFill.transform.localScale;fillPosition=progressFill.transform.localPosition;fillWidth=progressFill.bounds.size.x;ApplyProgress(0);
            if(SceneTransition.IsBusy)Tween.SetAlpha(overlay,0);
            else{Tween.SetAlpha(overlay,1);yield return Tween.Fade(overlay,0,0.3f);}
            yield return Tween.Wait(0.15f);AudioDirector.Play(SoundId.OvenRackSlide);yield return Tween.Move(carriage,Vector3.zero,0.9f,Easing.InOutCubic);
            Phase=BakingPhase.Waiting;hint.SetDelay(3f);
        }
        private void Update()
        {
            if(Phase!=BakingPhase.Waiting || SceneTransition.IsBusy)return;
            hint.Offer(new[]{(Vector2)power.position,(Vector2)power.position+new Vector2(0.02f,0.05f)});
            power.localScale=Vector3.one*(1+Mathf.Sin(Time.time*4)*0.025f);
            var p=PointerInput.Read(sceneCamera);Vector2 d=p.world-(Vector2)power.position;
            if(p.phase==PointerPhase.Began && Mathf.Abs(d.x)<0.7f && Mathf.Abs(d.y)<1.1f)StartCoroutine(Bake());
        }
        private void ApplyProgress(float value)
        {
            Progress=value;progressFill.transform.localScale=new Vector3(fillScale.x*value,fillScale.y,fillScale.z);
            progressFill.transform.localPosition=fillPosition+Vector3.left*(fillWidth*(1-value)*0.5f);
            pizza.SetBakedAmount(value);
        }
        private IEnumerator Bake()
        {
            Phase=BakingPhase.Baking;hint.Stop();AudioDirector.Play(SoundId.OvenPowerClick);power.localScale=Vector3.one;progressSparkles.SetEmission(true);
            yield return Tween.Run(7f,k=>
            {ApplyProgress(k);AudioDirector.Sustain(SoundId.OvenBakeLoop);flame.localScale=Vector3.one*(1+Mathf.Sin(Time.time*9)*0.08f);},Easing.Linear);
            progressSparkles.SetEmission(false);AudioDirector.Play(SoundId.OvenDoneDing);ApplyProgress(1);PizzaRecipeState.MarkCompleted();Phase=BakingPhase.Reward;
            yield return reward.Play();
            Phase=BakingPhase.Complete;onDishCompleted.Invoke();SceneTransition.Go("Kitchen");
        }
    }
}
