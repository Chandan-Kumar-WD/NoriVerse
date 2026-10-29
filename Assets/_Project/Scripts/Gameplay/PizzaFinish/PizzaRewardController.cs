using System.Collections;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public class PizzaRewardController:MonoBehaviour
    {
        [SerializeField] private SpriteRenderer dim;
        [SerializeField] private Transform plate,avatar;
        [SerializeField] private PizzaVisual hero;
        [SerializeField] private Transform[] stars;
        [SerializeField] private SpriteRenderer[] fills,glows;
        [SerializeField] private SpriteCrossfader avatarFace;
        [SerializeField] private SuccessBurst burst;
        [SerializeField] private ConfettiBurst confetti;
        public int StarsAwarded{get;private set;}
        public void Configure(SpriteRenderer shade,Transform dish,PizzaVisual pizza,Transform portrait,SpriteCrossfader face,Transform[] outlines,SpriteRenderer[] gold,SpriteRenderer[] halos,SuccessBurst fx,ConfettiBurst paper)
        {dim=shade;plate=dish;hero=pizza;avatar=portrait;avatarFace=face;stars=outlines;fills=gold;glows=halos;burst=fx;confetti=paper;}
        private void Awake()
        {
            Tween.SetAlpha(dim,0);plate.gameObject.SetActive(false);avatar.gameObject.SetActive(false);
            foreach(var s in stars)s.gameObject.SetActive(false);foreach(var f in fills)Tween.SetAlpha(f,0);foreach(var g in glows)Tween.SetAlpha(g,0);
        }
        private IEnumerator StarShimmer(Transform star,SpriteRenderer glow)
        {
            yield return Tween.Run(.8f,k=>
            {
                star.localRotation=Quaternion.Euler(0,0,Mathf.Sin(k*Mathf.PI*2)*5f*(1-k));
                Tween.SetAlpha(glow,.16f+Mathf.Sin(k*Mathf.PI)*.18f);
            });
            star.localRotation=Quaternion.identity;
        }
        public IEnumerator Play()
        {
            yield return Tween.Fade(dim,0.68f,0.4f);
            plate.gameObject.SetActive(true);hero.Rebuild();hero.SetBakedAmount(1);plate.localScale=Vector3.zero;
            yield return Tween.Scale(plate,Vector3.one,0.75f,k=>Easing.OutBack(k,1.0f));
            burst.Play(plate.position,Color.white,2.1f);AudioDirector.Play(SoundId.SuccessArpeggio);AudioDirector.Voice(SoundId.VoWow);
            foreach(var s in stars)
            {s.gameObject.SetActive(true);s.localScale=Vector3.zero;StartCoroutine(Tween.Scale(s,Vector3.one,0.3f,k=>Easing.OutBack(k,1.2f)));yield return Tween.Wait(0.18f);}
            avatar.gameObject.SetActive(true);avatar.localScale=Vector3.zero;StartCoroutine(Tween.Scale(avatar,Vector3.one,0.55f,Easing.OutCubic));
            yield return Tween.Wait(2.8f);
            for(int i=0;i<3;i++)
            {
                Tween.SetAlpha(fills[i],1);Tween.SetAlpha(glows[i],0.22f);StarsAwarded=i+1;
                stars[i].localScale=Vector3.one*0.72f;
                burst.Play(stars[i].position,new Color32(255,230,130,255),.8f);AudioDirector.Play(SoundId.StarDing,1f+i*.12f);if(i==2)AudioDirector.Voice(SoundId.VoYay);
                StartCoroutine(StarShimmer(stars[i],glows[i]));
                StartCoroutine(Tween.Scale(stars[i],Vector3.one,0.45f,k=>Easing.OutBack(k,1.5f)));
                avatarFace.Show(i==1?"satisfied":"smile",0.25f);
                yield return Tween.Wait(0.95f);
            }
            yield return Tween.Wait(1.5f);avatarFace.Show("satisfied",0.3f);
            burst.Play(plate.position,new Color32(255,220,90,255),2.0f);
            AudioDirector.Play(SoundId.ConfettiPop);yield return confetti.Play(plate.position);
            yield return Tween.Wait(0.6f);
        }
    }
}
