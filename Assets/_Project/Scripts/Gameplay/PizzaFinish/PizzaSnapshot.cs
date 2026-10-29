using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public static class PizzaSnapshot
    {
        /// <summary>Render the actual layered recipe for drag/hint sprites, rather than substituting a stock pizza.</summary>
        public static Sprite Create(PizzaVisual visual)
        {
            Transform t=visual.transform;Vector3 pos=t.position,scale=t.localScale;bool active=t.gameObject.activeSelf;
            t.gameObject.SetActive(true);visual.Rebuild();visual.SetBakedAmount(1);visual.SetOpacity(1);
            t.position=new Vector3(50,0,0);t.localScale=Vector3.one;
            var objects=visual.GetComponentsInChildren<Transform>(true);int[] layers=new int[objects.Length];
            for(int i=0;i<objects.Length;i++){layers[i]=objects[i].gameObject.layer;objects[i].gameObject.layer=31;}
            var go=new GameObject("PizzaSnapshotCamera");var cam=go.AddComponent<Camera>();cam.transform.position=new Vector3(50,0,-10);
            cam.orthographic=true;cam.orthographicSize=4.5f;cam.aspect=1;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.cullingMask=1<<31;
            var rt=new RenderTexture(512,512,24);var previous=RenderTexture.active;
            cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(512,512,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            tex.ReadPixels(new Rect(0,0,512,512),0,0);tex.Apply();
            cam.targetTexture=null;RenderTexture.active=previous;
            for(int i=0;i<objects.Length;i++)objects[i].gameObject.layer=layers[i];
            t.position=pos;t.localScale=scale;t.gameObject.SetActive(active);
            Object.Destroy(rt);Object.Destroy(go);
            return Sprite.Create(tex,new Rect(0,0,512,512),new Vector2(0.5f,0.5f),512f/3.05f);
        }
    }
}
