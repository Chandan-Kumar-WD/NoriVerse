using System.Collections;
using System.Collections.Generic;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public class MealReactionFx:MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sunCloud;
        private readonly List<SpriteRenderer> particles=new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> crumbs=new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> bubbleGlints=new List<SpriteRenderer>();
        private static Sprite bubble;
        public void Configure(SpriteRenderer weather){sunCloud=weather;Tween.SetAlpha(sunCloud,0);}
        private static Sprite Bubble()
        {
            if(bubble!=null)return bubble;
            const int n=96;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float r=Vector2.Distance(new Vector2(x+0.5f,y+0.5f),new Vector2(n/2f,n/2f))/(n/2f);
                float a=Mathf.Clamp01((0.055f-Mathf.Abs(r-0.88f))*n);
                t.SetPixel(x,y,new Color(1,1,1,a));
            }
            t.Apply();return bubble=Sprite.Create(t,new Rect(0,0,n,n),new Vector2(0.5f,0.5f),n);
        }
        public void Stop()
        {
            StopAllCoroutines();
            foreach(var crumb in crumbs)if(crumb!=null)Destroy(crumb.gameObject);crumbs.Clear();
            foreach(var p in particles)if(p!=null)Destroy(p.gameObject);particles.Clear();bubbleGlints.Clear();if(sunCloud!=null)Tween.SetAlpha(sunCloud,0);
        }
        public void PlayCrumbs(Vector2 mouth,int bite)
        {StartCoroutine(CrumbArc(mouth,bite));}
        private IEnumerator CrumbArc(Vector2 mouth,int bite)
        {
            var bits=new SpriteRenderer[5];var velocities=new Vector2[5];
            for(int i=0;i<bits.Length;i++)
            {
                var sr=new GameObject("BiteCrumb").AddComponent<SpriteRenderer>();sr.transform.SetParent(transform,false);sr.sprite=RuntimeSprites.Circle();sr.sortingOrder=125;
                sr.color=i%2==0?new Color(1,.77f,.32f,1):new Color(.80f,.42f,.13f,1);sr.transform.localScale=Vector3.one*(.045f+(i%3)*.016f);
                bits[i]=sr;crumbs.Add(sr);velocities[i]=new Vector2((i-2)*.23f,Random.Range(.22f,.55f));
            }
            yield return Tween.Run(.65f,k=>
            {
                for(int i=0;i<bits.Length;i++)
                {
                    bits[i].transform.position=mouth+velocities[i]*k+Vector2.down*(k*k*.55f);
                    Tween.SetAlpha(bits[i],1-k*k);
                }
            });
            foreach(var bit in bits){crumbs.Remove(bit);Destroy(bit.gameObject);}
        }
        public void Play(int serving,Vector2 head)
        {
            Stop();StartCoroutine(Animate(serving%3,head));
            StartCoroutine(Hum(serving%2==0?SoundId.VoGiggle:SoundId.VoHumHappy));
        }
        private IEnumerator Hum(SoundId line){yield return Tween.Wait(1.1f);AudioDirector.Voice(line);}
        private IEnumerator Animate(int type,Vector2 head)
        {
            int count=type==0?9:14;
            for(int i=0;i<count;i++)
            {
                var sr=new GameObject("MealReactionParticle").AddComponent<SpriteRenderer>();sr.transform.SetParent(transform,false);sr.sortingOrder=65+i%3;
                sr.sprite=type==1?RuntimeSprites.Sparkle():type==2?Bubble():RuntimeSprites.Circle();
                sr.color=type==0?new Color(.70f,.88f,.37f,0):type==2?new Color(.78f,.97f,1,0):new Color(1,.91f,.50f,0);particles.Add(sr);
                if(type==2)
                {
                    var glint=new GameObject("BubbleGlint").AddComponent<SpriteRenderer>();glint.transform.SetParent(sr.transform,false);glint.sprite=RuntimeSprites.Circle();glint.sortingOrder=68;glint.transform.localPosition=new Vector3(-.21f,.25f,0);glint.transform.localScale=Vector3.one*.22f;bubbleGlints.Add(glint);
                }
            }
            yield return Tween.Run(3.0f,k=>
            {
                if(type==0)
                {
                    float fade=Mathf.Clamp01(k*6)*Mathf.Clamp01((1-k)*5);
                    sunCloud.transform.position=head+new Vector2(Mathf.Lerp(1.8f,3.5f,k),.7f+Mathf.Sin(k*Mathf.PI)*.65f+k*.3f);
                    sunCloud.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(k*Mathf.PI*2)*6);
                    Tween.SetAlpha(sunCloud,fade);sunCloud.transform.localScale=Vector3.one*(.62f+.15f*Mathf.Sin(k*Mathf.PI));
                }
                for(int i=0;i<particles.Count;i++)
                {
                    float t=Mathf.Clamp01((k-i*.018f)/(1-i*.018f));float a=i*2.399963f;
                    Vector2 offset=type==0?new Vector2(1.8f+t*2f,Mathf.Sin(a)*.26f+t*.6f):new Vector2(Mathf.Cos(a)*1.9f+Mathf.Sin(t*4+a)*.15f,Mathf.Sin(a)*.55f+t*1.8f);
                    particles[i].transform.position=head+offset;
                    float size=type==0?.16f+.34f*t:type==2?.2f+i%4*.085f:.11f+(i%3)*.045f;
                    float alpha=Mathf.Clamp01(t*7)*Mathf.Clamp01((1-t)*4)*(type==0?.24f:.72f);
                    particles[i].transform.localScale=Vector3.one*size;particles[i].transform.localRotation=Quaternion.Euler(0,0,type==1?t*75:0);
                    Tween.SetAlpha(particles[i],alpha);
                    if(type==2)Tween.SetAlpha(bubbleGlints[i],alpha*.9f);
                }
            },Easing.Linear);
            foreach(var p in particles)Destroy(p.gameObject);particles.Clear();bubbleGlints.Clear();Tween.SetAlpha(sunCloud,0);sunCloud.transform.localRotation=Quaternion.identity;
        }
    }
}
