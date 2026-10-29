using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    /// <summary>Shared modal: pauses the current activity until the child cancels or returns home.</summary>
    [DefaultExecutionOrder(-1200)]
    public sealed class ExitConfirmation : MonoBehaviour
    {
        private static ExitConfirmation current;
        public static bool IsOpen => current != null;
        private Camera sceneCamera;
        private Transform panel;
        private float previousTimeScale;
        private int openedFrame;
        private string destination;
        public Vector2 ConfirmPosition => panel.TransformPoint(new Vector3(1.55f,-1.65f));
        public Vector2 CancelPosition => panel.TransformPoint(new Vector3(-1.55f,-1.65f));
        public Vector2 ClosePosition => panel.TransformPoint(new Vector3(3.65f,2.05f));

        public static void Show(Camera cam, string home = "Kitchen")
        {
            if (IsOpen || SceneTransition.IsBusy) return;
            var go = new GameObject("ExitConfirmation");
            current = go.AddComponent<ExitConfirmation>();
            current.sceneCamera = cam != null ? cam : Camera.main;
            current.destination = home;
            current.previousTimeScale = Time.timeScale;
            current.openedFrame = Time.frameCount;
            current.Build();
            Time.timeScale = 0;
        }
        private SpriteRenderer Shape(Transform parent,string name,Vector2 position,float width,float height,float radius,Color color,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=RuntimeSprites.RoundedRectangle(width,height,radius);sr.color=color;sr.sortingOrder=order;return sr;
        }
        private void Cross(Transform parent,Vector2 point,float size,Color color,int order)
        {
            foreach(float angle in new[]{45f,-45f})
            {
                var sr=Shape(parent,"Cross",point,size,.15f,.075f,color,order);
                sr.transform.localRotation=Quaternion.Euler(0,0,angle);
            }
        }
        private void Build()
        {
            Shape(transform,"Dim",Vector2.zero,1,1,0,new Color(.13f,.22f,.3f,.42f),30000);
            panel=new GameObject("Dialog").transform;panel.SetParent(transform,false);
            Shape(panel,"PinkBorder",Vector2.zero,7.6f,4.1f,.6f,new Color32(236,107,185,255),30001);
            Shape(panel,"SoftRim",Vector2.zero,7.22f,3.74f,.43f,new Color32(255,231,215,255),30002);
            Shape(panel,"CreamPanel",Vector2.zero,7.04f,3.56f,.36f,new Color32(255,247,240,255),30003);
            var label=new GameObject("Question");label.transform.SetParent(panel,false);label.transform.localPosition=new Vector3(0,.3f,0);
            var text=label.AddComponent<TextMesh>();text.text="Are you sure you\nwant to exit?";text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=80;text.fontStyle=FontStyle.Bold;text.characterSize=1;text.color=new Color32(215,77,158,255);
            var mesh=label.GetComponent<MeshRenderer>();mesh.sharedMaterial=text.font.material;mesh.sortingOrder=30005;
            float fit=Mathf.Min(5.9f/mesh.bounds.size.x,1.4f/mesh.bounds.size.y);label.transform.localScale=Vector3.one*fit;
            foreach(bool yes in new[]{false,true})
            {
                Vector2 p=new Vector2(yes?1.55f:-1.55f,-1.65f);
                Shape(panel,yes?"Confirm":"Cancel",p,2.1f,1.25f,.55f,yes?new Color32(28,187,53,255):new Color32(230,54,28,255),30006);
                Shape(panel,"ButtonFace",p+new Vector2(0,.04f),1.98f,1.08f,.48f,yes?new Color32(93,244,82,255):new Color32(255,102,56,255),30007);
                if(!yes)Cross(panel,p,.8f,Color.white,30009);
                else
                {
                    var a=Shape(panel,"CheckShort",p+new Vector2(-.22f,-.1f),.46f,.17f,.085f,Color.white,30009);a.transform.localRotation=Quaternion.Euler(0,0,-45);
                    var b=Shape(panel,"CheckLong",p+new Vector2(.16f,.04f),.94f,.17f,.085f,Color.white,30009);b.transform.localRotation=Quaternion.Euler(0,0,45);
                }
                Shape(panel,"Shine",p+new Vector2(-.55f,.39f),.3f,.055f,.027f,new Color(1,1,1,.7f),30009);
            }
            Shape(panel,"CloseRim",new Vector2(3.65f,2.05f),1.02f,1.02f,.51f,new Color32(234,110,188,255),30006);
            Shape(panel,"CloseFace",new Vector2(3.65f,2.05f),.86f,.86f,.43f,new Color32(255,246,224,255),30007);
            Cross(panel,new Vector2(3.65f,2.05f),.5f,new Color32(234,110,188,255),30009);
            Refit();
        }
        public void Refit()
        {
            if(sceneCamera==null)return;
            transform.position=new Vector3(sceneCamera.transform.position.x,sceneCamera.transform.position.y,0);
            float h=sceneCamera.orthographicSize*2,w=h*sceneCamera.aspect;
            transform.Find("Dim").localScale=new Vector3(h*Mathf.Max(sceneCamera.aspect,3.2f)+1,h+1,1);
            panel.localScale=Vector3.one*Mathf.Min(1,Mathf.Min((w-.6f)/8.3f,(h-.6f)/5.6f));
        }
        private void Update()
        {
            Refit();
            if(Time.frameCount<=openedFrame)return;
            var pointer=PointerInput.ReadUnblocked(sceneCamera);
            if(pointer.phase!=PointerPhase.Began)return;
            Vector2 local=panel.InverseTransformPoint(pointer.world);
            if(new Rect(.5f,-2.3f,2.1f,1.3f).Contains(local))Confirm();
            else if(new Rect(-2.6f,-2.3f,2.1f,1.3f).Contains(local)||Vector2.Distance(local,new Vector2(3.65f,2.05f))<.55f)Cancel();
        }
        public void Cancel(){AudioDirector.Play(SoundId.UiTap);Dismiss();}
        public void Confirm()
        {
            AudioDirector.Play(SoundId.ConfirmButton);string home=destination;Dismiss();SceneTransition.Go(home);
        }
        private void Dismiss()
        {
            if(current!=this)return;
            Time.timeScale=previousTimeScale;current=null;PointerInput.SuppressCurrentPress();gameObject.SetActive(false);Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if(current!=this)return;
            Time.timeScale=previousTimeScale;current=null;PointerInput.SuppressCurrentPress();
        }
    }
}
