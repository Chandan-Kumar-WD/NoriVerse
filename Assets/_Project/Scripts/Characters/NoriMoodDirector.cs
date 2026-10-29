using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Characters
{
    /// <summary>Mostly happy, unhurried home-screen behavior; only two subdued poses after sustained inactivity.</summary>
    public class NoriMoodDirector:MonoBehaviour
    {
        public const string Smile="smile",Blink="blink",HalfBlink="half_blink",BigSmile="big_smile",SmileWide="smile_wide";
        public const string Worried="worried",WorriedLeft="worried_left",WorriedRight="worried_right",WorriedClosed="worried_closed";
        public const string ArmsHalf="arms_half",SurprisedArms="surprised_arms",Sleepy="sleepy",YawnHalf="yawn_half",Yawn="yawn";
        [SerializeField] private SpriteCrossfader face;
        [SerializeField] private Transform headPivot;
        [SerializeField] private SpriteRenderer[] counterPaws;
        [SerializeField] private float idleSecondsBeforeSad=25f;
        private Vector3 restPosition;
        private float lastActivity;
        private bool sad,running;
        public bool IsSad=>sad;
        public float IdleSecondsBeforeSad=>idleSecondsBeforeSad;
        public void Configure(SpriteCrossfader crossfader,Transform pivot,SpriteRenderer[] paws)
        {face=crossfader;headPivot=pivot;counterPaws=paws;}
        private void Awake(){if(headPivot!=null)restPosition=headPivot.localPosition;}
        public void BeginLoop(){lastActivity=Time.time;Restart(false);}
        public void Stop(){StopAllCoroutines();running=false;sad=false;}
        public void NotifyActivity()
        {
            lastActivity=Time.time;
            if(running&&sad){AudioDirector.Voice(SoundId.VoGiggle);Restart(true);}
        }
        public void Cheer()
        {
            Stop();ResetHead();ShowPaws();face.Show(BigSmile,.55f);StartCoroutine(Bounce());
        }
        private void Restart(bool cheer)
        {
            Stop();running=true;ResetHead();ShowPaws();StartCoroutine(Sway());StartCoroutine(Run(cheer));
        }
        private void ShowPaws(){if(counterPaws!=null)foreach(var paw in counterPaws)Tween.SetAlpha(paw,1);}
        private void ResetHead()
        {if(headPivot!=null){headPivot.localPosition=restPosition;headPivot.localRotation=Quaternion.identity;headPivot.localScale=Vector3.one;}}
        private IEnumerator Run(bool cheer)
        {
            face.Show(cheer?BigSmile:Smile,.7f);
            if(cheer){yield return Tween.Wait(2.5f);face.Show(Smile,.7f);}
            float nextBlink=Time.time+Random.Range(5f,8f),nextHappy=Time.time+12f;
            while(Time.time-lastActivity<idleSecondsBeforeSad)
            {
                if(Time.time>=nextHappy)
                {
                    face.Show(SmileWide,.7f);
                    float until=Time.time+3.5f;
                    while(Time.time<until&&Time.time-lastActivity<idleSecondsBeforeSad)yield return null;
                    face.Show(Smile,.7f);nextHappy=Time.time+14f;
                }
                if(Time.time>=nextBlink)
                {yield return BlinkOnce();nextBlink=Time.time+Random.Range(6f,9f);}
                yield return null;
            }
            sad=true;
            while(true)
            {
                face.Show(Worried,.8f);AudioDirector.Voice(SoundId.VoHmmWorried);yield return Tween.Wait(12f);
                face.Show(Sleepy,.8f);AudioDirector.Voice(SoundId.VoYawn);yield return Tween.Wait(14f);
            }
        }
        private IEnumerator BlinkOnce()
        {
            string back=face.Current??Smile;
            face.Show(HalfBlink,.09f);yield return Tween.Wait(.10f);
            face.Show(Blink,.09f);yield return Tween.Wait(.13f);
            face.Show(HalfBlink,.09f);yield return Tween.Wait(.10f);
            face.Show(back,.14f);yield return Tween.Wait(.14f);
        }
        private IEnumerator Sway()
        {
            while(true)
            {
                float t=Time.time;
                if(headPivot!=null)
                {
                    headPivot.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*.55f)*.4f);
                    headPivot.localPosition=restPosition+new Vector3(Mathf.Sin(t*.4f)*.012f,Mathf.Sin(t*.8f)*.012f,0);
                }
                yield return null;
            }
        }
        private IEnumerator Bounce()
        {
            yield return Tween.Run(.65f,k=>
            {if(headPivot!=null)headPivot.localPosition=restPosition+Vector3.up*(Mathf.Sin(k*Mathf.PI)*.09f);},Easing.InOutQuad);
            ResetHead();
        }
    }
}
