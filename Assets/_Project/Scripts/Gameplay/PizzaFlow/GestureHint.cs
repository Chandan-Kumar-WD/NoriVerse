using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFlow
{
    /// <summary>A non-interactive ghost demonstrates a gesture after inactivity; it never changes game progress.</summary>
    public class GestureHint : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private SpriteRenderer hand, ghost;
        private Coroutine routine;
        private float lastActivity;
        [SerializeField] private float idleDelay=6f;
        public bool IsPlaying => routine != null;
        public void Configure(Camera cam, SpriteRenderer handSprite, SpriteRenderer ghostSprite)
        { sceneCamera = cam; hand = handSprite; ghost = ghostSprite; }
        private void Awake() { lastActivity = Time.time; Hide(); }
        private void Update()
        {
            if (PointerInput.Read(sceneCamera).IsDown) { lastActivity = Time.time; Stop(); }
        }
        public void ResetIdle() { lastActivity = Time.time; Stop(); }
        public void SetDelay(float seconds){idleDelay=Mathf.Max(0.5f,seconds);ResetIdle();}
        public bool PlayOnce(Vector2[] path,Sprite source=null,Vector3? scale=null)
        {
            if(routine!=null||path.Length<2)return false;
            routine=StartCoroutine(Animate(path,source,scale??Vector3.one,false));return true;
        }
        public void Offer(Vector2[] path, Sprite source = null, Vector3? scale = null, bool rotate = false)
        {
            if (routine == null && Time.time - lastActivity >= idleDelay && path.Length > 1)
                routine = StartCoroutine(Animate(path, source, scale ?? Vector3.one, rotate));
        }
        public void Stop()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            Hide();
        }
        private void Hide()
        {
            if (hand != null) Tween.SetAlpha(hand, 0);
            if (ghost != null) Tween.SetAlpha(ghost, 0);
        }
        private IEnumerator Animate(Vector2[] points, Sprite source, Vector3 scale, bool rotate)
        {
            ghost.sprite = source;
            ghost.transform.localScale = scale;
            float seconds = Mathf.Clamp(points.Length * 0.45f, 2.5f, 5f);
            yield return Tween.Run(seconds, k =>
            {
                float f = k * (points.Length - 1);
                int i = Mathf.Min(Mathf.FloorToInt(f), points.Length - 2);
                Vector2 p = Vector2.Lerp(points[i], points[i + 1], f - i);
                hand.transform.position = p;
                ghost.transform.position = p;
                if (rotate)
                {
                    Vector2 d = points[i + 1] - points[i];
                    ghost.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 90f);
                }
                float a = Mathf.Clamp01(k * 8f) * Mathf.Clamp01((1f - k) * 8f);
                Tween.SetAlpha(hand, a);
                Tween.SetAlpha(ghost, source == null ? 0 : a * 0.38f);
            }, Easing.Linear);
            Hide();
            lastActivity = Time.time;
            routine = null;
        }
    }
}
