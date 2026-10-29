using System;
using System.Collections;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class DoughRollingController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer small, medium, sheet, pin;
        [SerializeField] private GestureHint hint;
        [SerializeField] private float requiredTravel = 48f;
        private bool active, dragging, finishing;
        private Vector2 previous;
        private Transform hintBorder;
        private static readonly Vector2[] HintPath = { new Vector2(0,0), new Vector2(-3,2.8f), new Vector2(-3,-2.8f), new Vector2(3,2.8f), new Vector2(3,-2.8f), new Vector2(0,0) };
        public float Progress { get; private set; }
        public bool IsActive => active;
        public event Action Rolled;
        public void Configure(Camera cam, SpriteRenderer first, SpriteRenderer middle, SpriteRenderer last, SpriteRenderer tool, GestureHint gesture)
        { sceneCamera = cam; small = first; medium = middle; sheet = last; pin = tool; hint = gesture; }
        public void Begin() { active = true; hint.ResetIdle(); Apply(); }
        private void Awake()
        {
            Tween.SetAlpha(medium,0); Tween.SetAlpha(sheet,0);
            hintBorder=new GameObject("RollingHintBorder").transform;hintBorder.SetParent(transform,false);
            for(int i=0;i<88;i++)
            {
                int side=i/22;float k=(i%22+0.5f)/22f;
                Vector2 pos=side==0?new Vector2(Mathf.Lerp(-5.3f,5.3f,k),3.65f):side==1?new Vector2(5.3f,Mathf.Lerp(3.65f,-3.65f,k)):side==2?new Vector2(Mathf.Lerp(5.3f,-5.3f,k),-3.65f):new Vector2(-5.3f,Mathf.Lerp(-3.65f,3.65f,k));
                var dash=new GameObject("Dash").AddComponent<SpriteRenderer>();dash.transform.SetParent(hintBorder,false);dash.sprite=RuntimeSprites.Capsule();dash.sortingOrder=35;
                dash.transform.localPosition=pos;dash.transform.localScale=new Vector3(0.22f,0.22f,1);dash.transform.localRotation=Quaternion.Euler(0,0,side%2==0?0:90);
            }
            hintBorder.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (!active || finishing || SceneTransition.IsBusy) return;
            var p = PointerInput.Read(sceneCamera);
            if (p.phase == PointerPhase.Began)
            {
                Vector2 q = pin.transform.InverseTransformPoint(p.world);
                dragging = Mathf.Abs(q.x) < 4.3f && Mathf.Abs(q.y) < 1.2f;
                previous = p.world;
            }
            if (dragging && p.IsDown)
            {
                Vector2 delta = p.world - previous;
                if (delta.magnitude > 0.025f)
                {
                    float angle = Mathf.Atan2(delta.y,delta.x) * Mathf.Rad2Deg + 90f;
                    pin.transform.rotation = Quaternion.Slerp(pin.transform.rotation, Quaternion.Euler(0,0,angle), 1f - Mathf.Exp(-10f * Time.deltaTime));
                }
                Vector3 extent=pin.bounds.extents;
                float h=sceneCamera.orthographicSize,w=h*sceneCamera.aspect;
                Vector2 pos=new Vector2(Mathf.Clamp(p.world.x,Mathf.Max(-5.2f,-w+extent.x+0.15f),Mathf.Min(5.2f,w-extent.x-0.15f)),
                    Mathf.Clamp(p.world.y,Mathf.Max(-3.6f,-h+extent.y+0.15f),Mathf.Min(3.6f,h-extent.y-0.15f)));
                pin.transform.position = Vector2.Lerp(pin.transform.position, pos, 1f - Mathf.Exp(-22f * Time.deltaTime));
                // Correct immediately after rotation too, so the ends never cross the viewport during a fast turn.
                Vector3 pinPos=pin.transform.position;
                pinPos.x=Mathf.Clamp(pinPos.x,-w+extent.x+0.12f,w-extent.x-0.12f);
                pinPos.y=Mathf.Clamp(pinPos.y,-h+extent.y+0.12f,h-extent.y-0.12f);pin.transform.position=pinPos;
                float travel = Mathf.Min(Vector2.Distance(previous, p.world),0.6f);
                if (travel > 0.01f) AudioDirector.Sustain(SoundId.RollingPinLoop);
                if (Mathf.Abs(p.world.x) < 6f && Mathf.Abs(p.world.y) < 4.3f)
                    Progress = Mathf.Clamp01(Progress + travel / requiredTravel);
                previous = p.world;
                Apply();
                if (Progress >= 1f) StartCoroutine(Finish());
            }
            if (p.phase == PointerPhase.Ended) dragging = false;
            if (!dragging) hint.Offer(HintPath, pin.sprite, pin.transform.localScale, true);
            hintBorder.gameObject.SetActive(hint.IsPlaying);
        }
        private void Apply()
        {
            small.transform.localScale = Vector3.one * Mathf.Lerp(1f,1.45f,Progress);
            medium.transform.localScale = Vector3.one * Mathf.Lerp(0.82f,1.12f,Progress);
            sheet.transform.localScale = Vector3.one * Mathf.Lerp(0.82f,1f,Progress);
            float firstBlend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0.05f,0.45f,Progress));
            float lastBlend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0.55f,0.95f,Progress));
            Tween.SetAlpha(small,1f-firstBlend);
            Tween.SetAlpha(medium,firstBlend*(1f-lastBlend));
            Tween.SetAlpha(sheet,lastBlend);
        }
        private IEnumerator Finish()
        {
            finishing = true; active = false; hint.Stop();
            AudioDirector.Play(SoundId.SuccessArpeggio);
            hintBorder.gameObject.SetActive(false);
            yield return Tween.Move(pin.transform,new Vector3(0,8,0),0.65f,Easing.InOutCubic);
            pin.gameObject.SetActive(false);
            Rolled?.Invoke();
        }
    }
}
