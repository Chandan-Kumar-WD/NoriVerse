using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>Removes rounded bites from the player's rendered meal without substituting stock food.</summary>
    public sealed class MealBiteVisual:MonoBehaviour
    {
        private static readonly Vector2[] Centers={new Vector2(-.22f,.42f),new Vector2(.23f,.30f),new Vector2(0,-.12f)};
        private static readonly float[] Radii={.28f,.36f,.64f};
        private SpriteRenderer meal;
        private Texture2D texture;
        private Sprite sprite;
        private Color32[] original,pixels;
        public int BitesTaken{get;private set;}
        public void Configure(SpriteRenderer renderer,Sprite source)
        {
            meal=renderer;original=source.texture.GetPixels32();pixels=(Color32[])original.Clone();
            texture=new Texture2D(source.texture.width,source.texture.height,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            texture.SetPixels32(pixels);texture.Apply();
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),source.pixelsPerUnit);
        }
        public void ResetMeal()
        {BitesTaken=0;System.Array.Copy(original,pixels,original.Length);texture.SetPixels32(pixels);texture.Apply(false);meal.sprite=sprite;}
        public void Bite(int index,float progress)
        {
            int w=texture.width,h=texture.height;Vector2 c=Centers[index];float radius=Radii[index]*Mathf.SmoothStep(0,1,progress);
            int xmin=Mathf.Max(0,Mathf.FloorToInt((c.x+.5f-radius)*w)),xmax=Mathf.Min(w-1,Mathf.CeilToInt((c.x+.5f+radius)*w));
            int ymin=Mathf.Max(0,Mathf.FloorToInt((c.y+.5f-radius)*h)),ymax=Mathf.Min(h-1,Mathf.CeilToInt((c.y+.5f+radius)*h));
            for(int y=ymin;y<=ymax;y++)for(int x=xmin;x<=xmax;x++)
            {
                float d=Vector2.Distance(new Vector2((x+.5f)/w-.5f,(y+.5f)/h-.5f),c);
                byte a=(byte)(original[y*w+x].a*Mathf.Clamp01((d-radius)*w+1));
                if(a<pixels[y*w+x].a)pixels[y*w+x].a=a;
            }
            texture.SetPixels32(pixels);texture.Apply(false);
            if(progress>=.99f)BitesTaken=Mathf.Max(BitesTaken,index+1);
        }
        private void OnDestroy(){if(sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);}
    }
}
