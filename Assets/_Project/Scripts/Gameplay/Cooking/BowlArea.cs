using UnityEngine;

namespace NoriVerse.Gameplay.Cooking
{
    /// <summary>The circular drop zone inside the bowl.</summary>
    public class BowlArea : MonoBehaviour
    {
        [SerializeField] private float radius = 3.1f;

        public Vector2 Center => transform.position;
        public float Radius => radius;

        public void Configure(float dropRadius) => radius = dropRadius;

        public bool Contains(Vector2 point, float margin = 0f) => Vector2.Distance(point, Center) <= radius + margin;

        /// <summary>Pulls a point inside the bowl so landed items never hang over the rim.</summary>
        public Vector2 ClampInside(Vector2 point, float inset)
        {
            Vector2 offset = point - Center;
            float max = Mathf.Max(0.1f, radius - inset);
            return offset.magnitude > max ? Center + offset.normalized * max : point;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
