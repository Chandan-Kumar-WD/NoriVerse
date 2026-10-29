using System;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class DoughCutController : MonoBehaviour
    {
        public const float Radius = 3.2f, StartAngle = 125f;
        public static readonly Vector2 Center = new Vector2(0,-0.1f);
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private GestureHint hint;
        private Transform guide, marker;
        private LineRenderer cut;
        private bool active, dragging;
        private Vector2[] hintPath;
        private Material lineMaterial;
        public float Progress { get; private set; }
        public bool IsActive => active;
        public Vector2 MarkerPosition => Point(StartAngle - Progress * 360f);
        public event Action CutCompleted;
        public void Configure(Camera cam, GestureHint gesture) { sceneCamera = cam; hint = gesture; }
        private static Vector2 Point(float angle) => Center + new Vector2(Mathf.Cos(angle*Mathf.Deg2Rad),Mathf.Sin(angle*Mathf.Deg2Rad)) * Radius;
        public void Begin()
        {
            active = true; hint.ResetIdle();
            guide = new GameObject("CircularCutGuide").transform; guide.SetParent(transform,false);
            for (int i=0;i<48;i++)
            {
                float a=StartAngle-i*360f/48;
                var d=new GameObject("Dash").AddComponent<SpriteRenderer>(); d.transform.SetParent(guide,false);
                d.sprite=RuntimeSprites.Capsule(); d.sortingOrder=40; d.color=new Color(1,1,0.93f,0.95f);
                d.transform.position=Point(a); d.transform.localScale=new Vector3(0.25f,0.24f,1);
                d.transform.rotation=Quaternion.Euler(0,0,a-90f);
            }
            marker=new GameObject("TraceMarker").transform; marker.SetParent(transform,false);
            Circle(marker,"WhiteRing",0.61f,Color.white,45);
            Circle(marker,"BlueArrow",0.50f,new Color32(49,205,226,255),46);
            var arrow=new GameObject("Arrow").AddComponent<SpriteRenderer>(); arrow.transform.SetParent(marker,false);
            arrow.sprite=RuntimeSprites.Chevron(); arrow.sortingOrder=47; arrow.transform.localScale=Vector3.one*0.29f;
            cut=new GameObject("TracedCut").AddComponent<LineRenderer>(); cut.transform.SetParent(transform,false);
            lineMaterial=new Material(Shader.Find("Sprites/Default")); cut.sharedMaterial=lineMaterial;
            cut.startColor=cut.endColor=new Color32(226,155,58,255); cut.startWidth=cut.endWidth=0.12f;
            cut.numCapVertices=5; cut.sortingOrder=42; cut.useWorldSpace=true; cut.positionCount=0;
            hintPath=new Vector2[13]; for(int i=0;i<hintPath.Length;i++)hintPath[i]=Point(StartAngle-i*360f/(hintPath.Length-1));
            UpdateLine();
        }
        private static void Circle(Transform parent,string name,float size,Color color,int order)
        {
            var sr=new GameObject(name).AddComponent<SpriteRenderer>();sr.transform.SetParent(parent,false);
            sr.sprite=RuntimeSprites.Circle();sr.color=color;sr.sortingOrder=order;sr.transform.localScale=Vector3.one*size;
        }
        private void Update()
        {
            if(!active || SceneTransition.IsBusy)return;
            var p=PointerInput.Read(sceneCamera);
            if(p.phase==PointerPhase.Began)dragging=Vector2.Distance(p.world,MarkerPosition)<0.9f;
            if(dragging && p.IsDown)
            {
                Vector2 v=p.world-Center;
                if(Mathf.Abs(v.magnitude-Radius)<0.85f)
                {
                    float angle=Mathf.Atan2(v.y,v.x)*Mathf.Rad2Deg;
                    float delta=Mathf.DeltaAngle(angle,StartAngle-Progress*360f);
                    if(delta>=0 && delta<35f)
                    {
                        Progress=Mathf.Clamp01(Progress+delta/360f);
                        if(delta>0.05f)AudioDirector.Sustain(SoundId.DoughCutTrace);
                        if(Progress>0.993f)Progress=1;
                        UpdateLine();
                    }
                }
                if(Progress>=1)
                {
                    active=false;dragging=false;hint.Stop();guide.gameObject.SetActive(false);
                    marker.gameObject.SetActive(false);cut.gameObject.SetActive(false);CutCompleted?.Invoke();
                }
            }
            if(p.phase==PointerPhase.Ended)dragging=false;
            if(!dragging)hint.Offer(hintPath);
        }
        private void UpdateLine()
        {
            marker.position=MarkerPosition;marker.rotation=Quaternion.Euler(0,0,StartAngle-Progress*360f-90f);
            int count=Mathf.Max(2,Mathf.CeilToInt(Progress*128)+1);cut.positionCount=count;
            for(int i=0;i<count;i++)cut.SetPosition(i,Point(StartAngle-Progress*360f*i/(count-1)));
        }
        private void OnDestroy(){if(lineMaterial!=null)Destroy(lineMaterial);}
    }
}
