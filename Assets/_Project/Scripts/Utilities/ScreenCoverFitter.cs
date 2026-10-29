using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>Scales a unit-sized sprite so it always covers the whole camera view (e.g. the white fade).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class ScreenCoverFitter : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float margin = 1.1f;

        private SpriteRenderer spriteRenderer;
        public void Refit()=>LateUpdate();

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null) return;

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float viewHeight = targetCamera.orthographicSize * 2f;
            float viewWidth = viewHeight * targetCamera.aspect;
            Vector3 camPos = targetCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, transform.position.z);
            transform.localScale = new Vector3(viewWidth / spriteSize.x * margin, viewHeight / spriteSize.y * margin, 1f);
        }
    }
}
