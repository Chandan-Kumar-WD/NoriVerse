using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>Simple UI symbols/effects; all food, oven and character artwork is generated art.</summary>
    public static class FinishSymbols
    {
        private static Sprite power,flame;
        public static Sprite Power()
        {
            if(power!=null)return power;
            const int n=96;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                Vector2 p=new Vector2((x+0.5f-n/2f)/n,(y+0.5f-n/2f)/n);
                float angle=Mathf.Atan2(p.x,p.y)*Mathf.Rad2Deg;
                float ring=Mathf.Abs(angle)>35?Mathf.Clamp01((0.055f-Mathf.Abs(p.magnitude-0.29f))*n):0;
                float stick=Mathf.Abs(p.x)<0.055f && p.y>-0.02f && p.y<0.40f?1:0;
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Max(ring,stick)));
            }
            tex.Apply();return power=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(0.5f,0.5f),n);
        }
        public static Sprite Flame()
        {
            if(flame!=null)return flame;
            const int n=96;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float py=(y+0.5f)/n,px=(x+0.5f)/n;
                float width=py<0.35f?Mathf.Sqrt(Mathf.Max(0,1-Mathf.Pow((py-0.35f)/0.3f,2)))*0.3f:0.3f*Mathf.Pow(Mathf.Clamp01((0.94f-py)/0.59f),0.75f);
                float alpha=Mathf.Clamp01((width-Mathf.Abs(px-(0.5f+0.08f*py)))*n);
                Color c=Color.Lerp(new Color(1,0.88f,0.13f),new Color(1,0.32f,0.02f),py);c.a=alpha;tex.SetPixel(x,y,c);
            }
            tex.Apply();return flame=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(0.5f,0.5f),n);
        }
    }
}
