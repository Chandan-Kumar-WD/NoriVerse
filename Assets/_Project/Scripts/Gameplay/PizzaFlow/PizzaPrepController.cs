using System.Collections;
using NoriVerse.Core;
using NoriVerse.Utilities;
using NoriVerse.Gameplay.PizzaFinish;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public enum PizzaPrepPhase{Sauce,Cheese,Complete}
    public class PizzaPrepController:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer overlay,sauceIcon,cheeseIcon;
        [SerializeField] private SaucePaintController sauce;
        [SerializeField] private CheeseSprinkleController cheese;
        [SerializeField] private GestureHint hint;
        [SerializeField] private SuccessBurst burst;
        [SerializeField] private Transform confirm;
        private bool changing,confirmShown;
        private Vector3 confirmScale;
        private Vector2[] hintPath;
        private SpriteRenderer[] sauceBadge,cheeseBadge;
        private static void SetBadgeAlpha(SpriteRenderer[] sprites,float alpha)
        {foreach(var sprite in sprites)Tween.SetAlpha(sprite,alpha);}
        public PizzaPrepPhase Phase{get;private set;}
        public Vector2 ConfirmPosition=>confirm.position;
        public void Configure(Camera cam,SpriteRenderer cover,SpriteRenderer redIcon,SpriteRenderer yellowIcon,SaucePaintController painting,CheeseSprinkleController sprinkling,GestureHint gesture,SuccessBurst fx,Transform confirmButton)
        {sceneCamera=cam;overlay=cover;sauceIcon=redIcon;cheeseIcon=yellowIcon;sauce=painting;cheese=sprinkling;hint=gesture;burst=fx;confirm=confirmButton;}
        private IEnumerator Start()
        {
            AudioDirector.Music(SoundId.MusGameplayLoop);
            PizzaRecipeState.BeginNewRecipe();
            sauceBadge=sauceIcon.GetComponentsInChildren<SpriteRenderer>();cheeseBadge=cheeseIcon.GetComponentsInChildren<SpriteRenderer>();
            confirmScale=confirm.localScale;confirm.gameObject.SetActive(false);SetBadgeAlpha(cheeseBadge,0);
            sauce.Covered+=OnSauceCovered;
            hintPath=new Vector2[15];
            for(int i=0;i<hintPath.Length;i++)
            {float a=i*Mathf.PI*2/(hintPath.Length-1);hintPath[i]=new Vector2(-0.85f+Mathf.Cos(a)*2.2f,Mathf.Sin(a)*2.2f);}
            if(SceneTransition.IsBusy)Tween.SetAlpha(overlay,0);
            else{Tween.SetAlpha(overlay,1);yield return Tween.Fade(overlay,0,0.3f);}
            sauce.Begin();hint.ResetIdle();
        }
        private void OnSauceCovered(){if(!changing)StartCoroutine(ChangeToCheese());}
        private IEnumerator ChangeToCheese()
        {
            changing=true;hint.Stop();AudioDirector.Play(SoundId.SuccessArpeggio);yield return sauce.FinishCoat();
            burst.Play(new Vector2(-0.85f,0),new Color32(235,58,54,255),3.5f);
            yield return Tween.Wait(1.0f);
            yield return Tween.Run(0.45f,k=>{SetBadgeAlpha(sauceBadge,1-k);SetBadgeAlpha(cheeseBadge,k);});
            Phase=PizzaPrepPhase.Cheese;cheese.Begin();hint.ResetIdle();changing=false;
        }
        private void Update()
        {
            if(changing || Phase==PizzaPrepPhase.Complete || SceneTransition.IsBusy)return;
            hint.Offer(hintPath??System.Array.Empty<Vector2>());
            if(Phase!=PizzaPrepPhase.Cheese)return;
            if(cheese.CanConfirm && !confirmShown)
            {confirmShown=true;AudioDirector.Play(SoundId.ItemPopIn);confirm.gameObject.SetActive(true);confirm.localScale=Vector3.zero;StartCoroutine(Tween.Scale(confirm,confirmScale,0.3f,k=>Easing.OutBack(k,1.5f)));}
            var p=PointerInput.Read(sceneCamera);
            Vector2 delta=p.world-(Vector2)confirm.position;
            if(confirmShown && p.phase==PointerPhase.Began && Mathf.Abs(delta.x)<1.15f && Mathf.Abs(delta.y)<0.75f)
            {AudioDirector.Play(SoundId.ConfirmButton);Phase=PizzaPrepPhase.Complete;PizzaRecipeState.IsPrepared=true;cheese.Stop();hint.Stop();SceneTransition.Go("ToppingSort");}
        }
    }
}
