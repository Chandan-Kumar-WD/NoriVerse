using System;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    /// <summary>Reveals generated sauce only under the finger. Coverage counts unique pixels, never repeat strokes.</summary>
    public class SaucePaintController : MonoBehaviour
    {
        private const int Size=512;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer sauce;
        [SerializeField] private float brushRadius=0.68f;
        private Texture2D painted;
        private Sprite runtimeSprite;
        private Color32[] source,colors;
        private bool[] covered;
        private int total,filled;
        private bool active,dragging;
        private Vector2 previous;
        public float Coverage=>total==0?0:(float)filled/total;
        public bool IsActive=>active;
        public Vector2 Center= new Vector2(-0.85f,0);
        public event Action Covered;
        public void Configure(Camera cam,SpriteRenderer render){sceneCamera=cam;sauce=render;}
        private void Awake()=>Prepare();
        private void Prepare()
        {
            var original=sauce.sprite;
            source=new Color32[Size*Size];colors=new Color32[source.Length];covered=new bool[source.Length];
            Rect r=original.textureRect;
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)
            {
                int i=y*Size+x;
                Color c=original.texture.GetPixelBilinear((r.x+(x+0.5f)/Size*r.width)/original.texture.width,(r.y+(y+0.5f)/Size*r.height)/original.texture.height);
                source[i]=c;colors[i]=new Color32(source[i].r,source[i].g,source[i].b,0);
                if(source[i].a>128)total++;
            }
            painted=new Texture2D(Size,Size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            painted.SetPixels32(colors);painted.Apply();
            runtimeSprite=Sprite.Create(painted,new Rect(0,0,Size,Size),new Vector2(0.5f,0.5f),Size/7.05f);
            sauce.sprite=runtimeSprite;
        }
        public void Begin(){active=true;}
        private void Update()
        {
            if(!active || SceneTransition.IsBusy)return;
            var p=PointerInput.Read(sceneCamera);
            if(p.phase==PointerPhase.Began){dragging=Vector2.Distance(p.world,Center)<3.65f;previous=p.world;}
            if(dragging && p.IsDown)
            {
                int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(previous,p.world)/0.2f));
                steps=Mathf.Min(steps,60);
                bool dirty=false;
                for(int i=1;i<=steps;i++)dirty|=Brush(Vector2.Lerp(previous,p.world,(float)i/steps));
                previous=p.world;
                if(dirty){painted.SetPixels32(colors);painted.Apply(false);}
                if(steps>1||dirty)AudioDirector.Sustain(SoundId.SauceSquishLoop);
                if(Coverage>=0.90f){active=false;dragging=false;Covered?.Invoke();}
            }
            if(p.phase==PointerPhase.Ended)dragging=false;
        }
        private bool Brush(Vector2 world)
        {
            Vector2 local=sauce.transform.InverseTransformPoint(world);
            float x=(local.x/7.05f+0.5f)*Size,y=(local.y/7.05f+0.5f)*Size;
            float r=brushRadius/7.05f*Size;
            bool dirty=false;
            for(int py=Mathf.Max(0,Mathf.FloorToInt(y-r));py<=Mathf.Min(Size-1,Mathf.CeilToInt(y+r));py++)
                for(int px=Mathf.Max(0,Mathf.FloorToInt(x-r));px<=Mathf.Min(Size-1,Mathf.CeilToInt(x+r));px++)
                {
                    float d=Vector2.Distance(new Vector2(px+0.5f,py+0.5f),new Vector2(x,y));
                    if(d>r)continue;
                    int i=py*Size+px;
                    byte alpha=(byte)(source[i].a*Mathf.Clamp01((r-d)*0.4f));
                    if(alpha<=colors[i].a)continue;
                    colors[i].a=alpha;dirty=true;
                    if(!covered[i] && alpha>128 && source[i].a>128){covered[i]=true;filled++;}
                }
            return dirty;
        }
        public System.Collections.IEnumerator FinishCoat()
        {
            var start=(Color32[])colors.Clone();
            yield return Tween.Run(0.35f,k=>
            {
                for(int i=0;i<colors.Length;i++)colors[i].a=(byte)Mathf.Lerp(start[i].a,source[i].a,k);
                painted.SetPixels32(colors);painted.Apply(false);
            });
        }
        private void OnDestroy(){if(runtimeSprite!=null)Destroy(runtimeSprite);if(painted!=null)Destroy(painted);}
    }
}
