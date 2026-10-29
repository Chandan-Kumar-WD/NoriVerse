using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public class ConfettiBurst:MonoBehaviour
    {
        public IEnumerator Play(Vector2 center)
        {
            const int count=48;var pieces=new SpriteRenderer[count];var velocities=new Vector2[count];
            for(int i=0;i<count;i++)
            {
                var go=new GameObject("Confetti");go.transform.SetParent(transform,false);
                pieces[i]=go.AddComponent<SpriteRenderer>();pieces[i].sprite=RuntimeSprites.Capsule();pieces[i].sortingOrder=760;
                pieces[i].color=Color.HSVToRGB((i*0.137f)%1f,0.65f,1f);go.transform.localScale=new Vector3(0.16f,0.22f,1);
                velocities[i]=new Vector2(Random.Range(-5.5f,5.5f),Random.Range(1.8f,5.8f));
            }
            yield return Tween.Run(1.5f,k=>
            {
                float t=k*1.5f;
                for(int i=0;i<count;i++)
                {
                    pieces[i].transform.position=center+velocities[i]*t+Vector2.down*(2.8f*t*t);
                    pieces[i].transform.rotation=Quaternion.Euler(0,0,i*33+k*360);
                    Tween.SetAlpha(pieces[i],Mathf.Clamp01((1-k)*3));
                }
            },Easing.Linear);
            foreach(var p in pieces)Destroy(p.gameObject);
        }
    }
}
