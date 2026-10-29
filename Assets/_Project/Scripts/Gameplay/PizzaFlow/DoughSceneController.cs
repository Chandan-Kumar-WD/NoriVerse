using System.Collections;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class DoughSceneController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer overlay, roundBase;
        [SerializeField] private SpriteRenderer[] doughStages;
        [SerializeField] private Transform pin;
        [SerializeField] private DoughRollingController rolling;
        [SerializeField] private DoughCutController cutting;
        [SerializeField] private SuccessBurst burst;
        public void Configure(SpriteRenderer cover,SpriteRenderer pizza,SpriteRenderer[] stages,Transform tool,DoughRollingController roll,DoughCutController cut,SuccessBurst fx)
        {overlay=cover;roundBase=pizza;doughStages=stages;pin=tool;rolling=roll;cutting=cut;burst=fx;}
        private IEnumerator Start()
        {
            AudioDirector.Music(SoundId.MusGameplayLoop);
            Tween.SetAlpha(roundBase,0);
            rolling.Rolled+=cutting.Begin;cutting.CutCompleted+=OnCut;
            if(SceneTransition.IsBusy)Tween.SetAlpha(overlay,0);
            else{Tween.SetAlpha(overlay,1);yield return Tween.Fade(overlay,0,0.3f);}
            yield return Tween.Wait(0.45f);
            yield return Tween.Move(pin,Vector3.zero,0.55f,Easing.OutCubic);
            rolling.Begin();
        }
        private void OnCut()=>StartCoroutine(Finish());
        private IEnumerator Finish()
        {
            AudioDirector.Play(SoundId.CutCompleteSparkle);
            AudioDirector.Voice(SoundId.VoYay);
            burst.Play(DoughCutController.Center,Color.white,3.3f);
            yield return Tween.Run(0.75f,k=>
            {
                foreach(var s in doughStages)Tween.SetAlpha(s,1-k);
                // Only the final sheet should fade away; the earlier dough shapes were already hidden.
                for(int i=0;i<doughStages.Length-1;i++)Tween.SetAlpha(doughStages[i],0);
                Tween.SetAlpha(roundBase,k);
            });
            yield return Tween.Wait(0.8f);
            SceneTransition.Go("PizzaPrep");
        }
    }
}
