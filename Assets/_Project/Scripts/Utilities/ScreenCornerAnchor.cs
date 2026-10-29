using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>Keeps a sprite inside the upper-left safe corner after the game camera has fitted the scene.</summary>
    [ExecuteAlways, DefaultExecutionOrder(100)]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScreenCornerAnchor : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private float margin = 0.35f;
        [SerializeField] private bool anchorBottom;

        public void Configure(Camera camera, float inset = 0.35f, bool bottomLeft = false)
        {
            sceneCamera = camera;
            margin = inset;
            anchorBottom=bottomLeft;
            Refresh();
        }

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (sceneCamera == null || !sceneCamera.orthographic) return;
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite.sprite == null) return;
            Rect safe = Screen.safeArea;
            float left = Screen.width > 0 ? safe.xMin / Screen.width : 0f;
            float top = Screen.height > 0 ? (anchorBottom?safe.yMin:safe.yMax) / Screen.height : (anchorBottom?0f:1f);
            Vector3 corner = sceneCamera.ViewportToWorldPoint(new Vector3(left, top, 0));
            Vector3 half = sprite.bounds.extents;
            transform.position = new Vector3(corner.x + margin + half.x, anchorBottom?corner.y+margin+half.y:corner.y-margin-half.y, transform.position.z);
        }
    }
}
