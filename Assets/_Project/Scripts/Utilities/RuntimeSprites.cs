using UnityEngine;
using System.Collections.Generic;

namespace NoriVerse.Utilities
{
    /// <summary>Small procedural sprites for effects (touch dot, hint-arrow dashes) so they need no art files.</summary>
    public static class RuntimeSprites
    {
        private static Sprite circle, capsule, chevron, sparkle;
        private static readonly Dictionary<Vector3,Sprite> rounded=new Dictionary<Vector3,Sprite>();

        public static Sprite RoundedRectangle(float width,float height,float radius)
        {
            Vector3 key=new Vector3(width,height,radius);
            if(rounded.TryGetValue(key,out var cached)&&cached!=null)return cached;
            const float ppu=128f;
            int w=Mathf.CeilToInt(width*ppu),h=Mathf.CeilToInt(height*ppu);
            radius=Mathf.Min(radius,Mathf.Min(width,height)*0.5f);
            var tex=NewTexture(w,h);
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                float dx=Mathf.Abs((x+0.5f)/ppu-width*0.5f)-(width*0.5f-radius);
                float dy=Mathf.Abs((y+0.5f)/ppu-height*0.5f)-(height*0.5f-radius);
                float d=new Vector2(Mathf.Max(dx,0),Mathf.Max(dy,0)).magnitude+Mathf.Min(Mathf.Max(dx,dy),0)-radius;
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(0.5f-d*ppu)));
            }
            tex.Apply();
            var sprite=Sprite.Create(tex,new Rect(0,0,w,h),new Vector2(0.5f,0.5f),ppu);
            rounded[key]=sprite;return sprite;
        }

        public static Sprite Sparkle()
        {
            if (sparkle != null) return sparkle;
            const int n=96;
            var tex=NewTexture(n,n);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float nx=Mathf.Abs((x+0.5f-n/2f)/(n/2f));
                float ny=Mathf.Abs((y+0.5f-n/2f)/(n/2f));
                float edge=1f-Mathf.Sqrt(nx)-Mathf.Sqrt(ny);
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(edge*25f)));
            }
            tex.Apply();
            return sparkle=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(0.5f,0.5f),n);
        }

        /// <summary>Soft-edged white circle, 1 unit across.</summary>
        public static Sprite Circle()
        {
            if (circle != null) return circle;
            const int n = 64;
            var tex = NewTexture(n, n);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1f - d) * 12f)));
                }
            tex.Apply();
            return circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        /// <summary>White rounded dash, 1 unit long and 0.28 unit thick.</summary>
        public static Sprite Capsule()
        {
            if (capsule != null) return capsule;
            const int w = 96, h = 28;
            var tex = NewTexture(w, h);
            float r = h / 2f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, r, w - r);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, r));
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((r - d) * 3f)));
                }
            tex.Apply();
            return capsule = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), w);
        }

        /// <summary>A thick arrowhead ("&gt;") pointing right, ~1 unit tall.</summary>
        public static Sprite Chevron()
        {
            if (chevron != null) return chevron;
            const int n = 96;
            var tex = NewTexture(n, n);
            float thick = n * 0.13f;
            Vector2 tip = new Vector2(n * 0.78f, n * 0.5f), a = new Vector2(n * 0.22f, n * 0.88f), b = new Vector2(n * 0.22f, n * 0.12f);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Mathf.Min(DistanceToSegment(p, a, tip), DistanceToSegment(p, b, tip));
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((thick - d) * 2.5f)));
                }
            tex.Apply();
            return chevron = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        private static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
