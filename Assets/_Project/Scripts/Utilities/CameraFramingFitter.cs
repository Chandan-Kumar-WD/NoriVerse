using UnityEngine;

namespace NoriVerse.Utilities
{
    /// <summary>
    /// Keeps a fixed-width strip of the world visible on any aspect ratio so landscape layouts never get
    /// cropped on narrow tablets, while taller-than-wide phones still show the full design height.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class CameraFramingFitter : MonoBehaviour
    {
        [SerializeField] private float designHeight = 10f;
        [SerializeField] private float designWidth = 13f;

        private Camera cam;
        private float lastAspect;

        private void OnEnable() => Fit();

        private void LateUpdate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (!Mathf.Approximately(lastAspect, cam.aspect)) Fit();
        }

        /// <summary>Re-fits immediately (e.g. after the aspect ratio changed in the same frame).</summary>
        public void Refit() => Fit();

        private void Fit()
        {
            cam = GetComponent<Camera>();
            lastAspect = cam.aspect;
            float sizeForWidth = designWidth / (2f * cam.aspect);
            cam.orthographicSize = Mathf.Max(designHeight / 2f, sizeForWidth);
        }
    }
}
