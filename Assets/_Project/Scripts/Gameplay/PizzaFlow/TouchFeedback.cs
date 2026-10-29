using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class TouchFeedback:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        private SpriteRenderer dot;
        public void Configure(Camera cam)=>sceneCamera=cam;
        private void Awake()
        {
            dot=new GameObject("TouchDot").AddComponent<SpriteRenderer>();dot.transform.SetParent(transform,false);
            dot.sprite=RuntimeSprites.Circle();dot.sortingOrder=190;dot.transform.localScale=Vector3.one*0.24f;
            dot.color=new Color(0.4f,0.43f,0.45f,0);
        }
        private void Update()
        {
            var p=PointerInput.Read(sceneCamera);dot.transform.position=p.world;Tween.SetAlpha(dot,p.IsDown?0.30f:0);
        }
    }
}
