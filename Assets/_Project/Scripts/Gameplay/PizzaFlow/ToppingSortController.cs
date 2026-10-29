using System.Collections;
using System.Collections.Generic;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using UnityEngine.Events;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class ToppingSortController:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer overlay;
        [SerializeField] private SortIngredient[] ingredients;
        [SerializeField] private SortTarget[] targets;
        [SerializeField] private GestureHint hint;
        [SerializeField] private UnityEvent onCompleted=new UnityEvent();
        private SortIngredient dragging;
        private Vector2 offset;
        private bool active;
        private int accepting;
        private static readonly ToppingKind[] HintOrder={ToppingKind.Salami,ToppingKind.Cheese,ToppingKind.Basil,ToppingKind.Tomato};
        public int CompletedCount{get;private set;}
        public bool IsComplete=>CompletedCount==4;
        public UnityEvent OnCompleted=>onCompleted;
        public IReadOnlyList<SortIngredient> Ingredients=>ingredients;
        public IReadOnlyList<SortTarget> Targets=>targets;
        public void Configure(Camera cam,SpriteRenderer cover,SortIngredient[] food,SortTarget[] plates,GestureHint gesture)
        {sceneCamera=cam;overlay=cover;ingredients=food;targets=plates;hint=gesture;}
        private IEnumerator Start()
        {
            AudioDirector.Music(SoundId.MusGameplayLoop);
            if(SceneTransition.IsBusy)Tween.SetAlpha(overlay,0);
            else{Tween.SetAlpha(overlay,1);yield return Tween.Fade(overlay,0,0.3f);}
            for(int i=0;i<ingredients.Length;i++)
            {
                Vector3 scale=ingredients[i].transform.localScale;ingredients[i].transform.localScale=Vector3.zero;
                StartCoroutine(Pop(ingredients[i].transform,scale,i*0.025f));
            }
            yield return Tween.Wait(0.5f);
            for(int i=0;i<targets.Length;i++)StartCoroutine(Pop(targets[i].transform,Vector3.one,i*0.1f));
            active=true;hint.ResetIdle();
        }
        private IEnumerator Pop(Transform item,Vector3 scale,float delay)
        {yield return Tween.Wait(delay);AudioDirector.Play(SoundId.ItemPopIn,Random.Range(0.92f,1.12f),0.05f);yield return Tween.Scale(item,scale,0.35f,k=>Easing.OutBack(k,1.3f));}
        private void Update()
        {
            if(!active || SceneTransition.IsBusy || IsComplete)return;
            var p=PointerInput.Read(sceneCamera);
            if(p.phase==PointerPhase.Began && dragging==null)
            {
                SortIngredient picked=null;
                foreach(var item in ingredients)
                    if(item.Available && item.GetComponent<Collider2D>().enabled && item.GetComponent<Collider2D>().OverlapPoint(p.world))
                        if(picked==null || item.Render.sortingOrder>picked.Render.sortingOrder)picked=item;
                if(picked!=null)
                {
                    dragging=picked;offset=(Vector2)picked.transform.position-p.world;
                    picked.Render.sortingOrder=130;hint.Stop();AudioDirector.Play(SoundId.ItemPickup);
                }
            }
            if(dragging!=null && p.IsDown)
            {
                Vector3 before=dragging.transform.position;Vector2 to=p.world+offset;
                dragging.transform.position=to;
                float tilt=Mathf.Clamp((to.x-before.x)*-15f,-12,12);
                dragging.transform.rotation=Quaternion.Lerp(dragging.transform.rotation,Quaternion.Euler(0,0,tilt),0.4f);
            }
            if(dragging!=null && p.phase==PointerPhase.Ended)
            {
                var item=dragging;dragging=null;
                SortTarget target=null;
                foreach(var plate in targets)
                    if(!plate.IsCompleted && Vector2.Distance(p.world,plate.DropPoint)<1.4f){target=plate;break;}
                item.GetComponent<Collider2D>().enabled=false;
                if(target!=null && target.Kind==item.Kind)
                {AudioDirector.Play(SoundId.SortCorrectDing);target.MarkCompleted();item.Available=false;CompletedCount++;accepting++;StartCoroutine(Accept(item,target));}
                else{AudioDirector.Play(SoundId.DropReturn);StartCoroutine(Return(item));}
            }
            if(dragging==null)
            {
                foreach(var kind in HintOrder)
                {
                    SortTarget target=null;
                    foreach(var candidate in targets)if(!candidate.IsCompleted && candidate.Kind==kind){target=candidate;break;}
                    if(target==null)continue;
                    SortIngredient first=null;
                    foreach(var item in ingredients)if(item.Available && item.Kind==target.Kind){first=item;break;}
                    if(first!=null)hint.Offer(new[]{(Vector2)first.transform.position,target.DropPoint},first.Render.sprite,first.transform.localScale);
                    break;
                }
            }
        }
        private IEnumerator Return(SortIngredient item)
        {
            Quaternion from=item.transform.rotation;
            Vector3 start=item.transform.position;
            yield return Tween.Run(0.45f,k=>
            {item.transform.position=Vector3.Lerp(start,item.Home,k);item.transform.rotation=Quaternion.Slerp(from,item.HomeRotation,k);},Easing.OutCubic);
            item.Render.sortingOrder=20;item.GetComponent<Collider2D>().enabled=true;
        }
        private IEnumerator Accept(SortIngredient item,SortTarget target)
        {
            hint.Stop();
            yield return Tween.Move(item.transform,target.DropPoint,0.22f,Easing.OutCubic);
            item.transform.SetParent(target.transform,true);item.transform.rotation=Quaternion.identity;
            item.Render.sortingOrder=55;
            yield return target.Leave();
            accepting--;
            if(IsComplete)
            {
                if(!active || accepting>0)yield break;
                active=false;AudioDirector.Play(SoundId.SuccessArpeggio);AudioDirector.Voice(SoundId.VoYay);yield return Tween.Wait(0.45f);onCompleted.Invoke();SceneTransition.Go("PizzaDecorate");
            }
            else
            {
                var remaining=new List<SortTarget>();foreach(var t in targets)if(!t.IsCompleted)remaining.Add(t);
                for(int i=0;i<remaining.Count;i++)
                    StartCoroutine(Tween.Move(remaining[i].transform,new Vector3((i-(remaining.Count-1)*0.5f)*3.3f,-3.3f,0),0.45f,Easing.InOutCubic));
                hint.ResetIdle();
            }
        }
    }
}
