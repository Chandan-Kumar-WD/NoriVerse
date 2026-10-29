using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>A small reusable pool of twinkles follows the actual advancing progress edge.</summary>
    public sealed class BakingProgressSparkles:MonoBehaviour
    {
        private sealed class Twinkle
        {
            public SpriteRenderer star,halo;
            public Vector2 origin,velocity;
            public float age,lifetime,size,spin;
        }
        private readonly Twinkle[] particles=new Twinkle[14];
        private SpriteRenderer fill;
        private bool emitting;
        private float nextEmission;
        private int cursor;
        public int EmittedCount{get;private set;}
        public void Configure(SpriteRenderer progress){fill=progress;}
        private void Awake()
        {
            for(int i=0;i<particles.Length;i++)
            {
                var root=new GameObject("HeatTwinkle"+i);root.transform.SetParent(transform,false);
                var star=root.AddComponent<SpriteRenderer>();star.sprite=RuntimeSprites.Sparkle();star.sortingOrder=76;
                var halo=new GameObject("SoftGlow").AddComponent<SpriteRenderer>();halo.transform.SetParent(root.transform,false);halo.sprite=RuntimeSprites.Circle();halo.sortingOrder=75;halo.transform.localScale=Vector3.one*1.8f;
                Tween.SetAlpha(star,0);Tween.SetAlpha(halo,0);
                particles[i]=new Twinkle{star=star,halo=halo,age=2,lifetime=1};
            }
        }
        public void SetEmission(bool enabled){emitting=enabled;if(enabled)nextEmission=Time.time;}
        private void Update()
        {
            if(Time.deltaTime<=0)return;
            if(emitting&&fill!=null&&Time.time>=nextEmission)
            {
                nextEmission=Time.time+.075f;
                var p=particles[cursor++%particles.Length];p.age=0;p.lifetime=Random.Range(.45f,.8f);p.size=Random.Range(.24f,.42f);p.spin=Random.Range(-80f,80f);
                p.origin=new Vector2(fill.bounds.max.x,fill.bounds.center.y)+Random.insideUnitCircle*.11f;
                p.velocity=new Vector2(Random.Range(-.35f,.35f),Random.Range(.65f,1.20f));EmittedCount++;
                p.star.color=new Color(1,.98f,.76f,0);p.halo.color=new Color(1,.65f,.12f,0);
            }
            foreach(var p in particles)
            {
                p.age+=Time.deltaTime;float k=Mathf.Clamp01(p.age/p.lifetime);
                float alpha=Mathf.Sin(k*Mathf.PI);
                p.star.transform.position=p.origin+p.velocity*p.age;
                p.star.transform.localScale=Vector3.one*(p.size*(.55f+.45f*alpha));p.star.transform.localRotation=Quaternion.Euler(0,0,p.spin*p.age);
                Tween.SetAlpha(p.star,alpha);Tween.SetAlpha(p.halo,alpha*.2f);
            }
        }
    }
}
