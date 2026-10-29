using System;
using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>Aligned expression beats plus restrained follow-through; counter paws remain planted.</summary>
    public sealed class NoriEatingAnimator:MonoBehaviour
    {
        private SpriteCrossfader face;
        private Vector3 rest;
        public string Stage{get;private set;}="Idle";
        public void Configure(SpriteCrossfader expressions){face=expressions;rest=face.transform.localPosition;}
        private bool wasNear;
        public void Anticipate(bool near)
        {
            if(near&&!wasNear)AudioDirector.Voice(SoundId.VoMmmAnticipate);
            wasNear=near;Stage="Anticipation";face.Show(near?"eat_open":"eat_anticipate",.28f);
        }
        public void ResetPose(){wasNear=false;face.transform.localPosition=rest;face.transform.localRotation=Quaternion.identity;Stage="Idle";}
        public IEnumerator Eat(Action<int,float> bite,Action<int> crumbs)
        {
            AudioDirector.Voice(SoundId.VoNom);
            for(int i=0;i<3;i++)
            {
                Stage="Bite";face.Show("eat_open",.22f);
                yield return Tween.Run(.24f,k=>Pose(.025f*Mathf.Sin(k*Mathf.PI),-.45f*Mathf.Sin(k*Mathf.PI)));
                face.Show("eat_bite",.24f);crumbs(i);
                yield return Tween.Run(.30f,k=>{bite(i,k);Pose(-.025f*Mathf.Sin(k*Mathf.PI),.55f*Mathf.Sin(k*Mathf.PI));});
                if(i==1)AudioDirector.Voice(SoundId.VoChew);
                Stage="Chewing";face.Show(i%2==0?"eat_chew_left":"eat_chew_right",.24f);
                yield return Tween.Run(.32f,k=>Pose(.035f*Mathf.Sin(k*Mathf.PI*2),(i%2==0?1:-1)*.65f*Mathf.Sin(k*Mathf.PI)));
            }
            Stage="Swallow";face.Show("eat_bite",.25f);
            yield return Tween.Run(.32f,k=>Pose(-.045f*Mathf.Sin(k*Mathf.PI),0));
            Stage="Satisfied";face.Show("eat_satisfied",.3f);AudioDirector.Voice(SoundId.VoYummy);
            yield return Tween.Run(.55f,k=>Pose(.09f*Mathf.Sin(k*Mathf.PI),-.6f*Mathf.Sin(k*Mathf.PI)));
            ResetPose();
        }
        private void Pose(float lift,float angle)
        {face.transform.localPosition=rest+new Vector3(0,lift,0);face.transform.localRotation=Quaternion.Euler(0,0,angle);}
    }
}
