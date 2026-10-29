using System.Collections;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class SortTarget:MonoBehaviour
    {
        [SerializeField] private ToppingKind kind;
        [SerializeField] private SpriteRenderer silhouette;
        [SerializeField] private Transform check;
        public ToppingKind Kind=>kind;
        public bool IsCompleted{get;private set;}
        public Vector2 DropPoint=>transform.position;
        public void Configure(ToppingKind value,SpriteRenderer grey,Transform mark){kind=value;silhouette=grey;check=mark;}
        private void Awake(){check.gameObject.SetActive(false);}
        public void MarkCompleted()
        {IsCompleted=true;Tween.SetAlpha(silhouette,0);check.gameObject.SetActive(true);}
        public IEnumerator Leave()
        {
            yield return Tween.Wait(0.6f);
            yield return Tween.Run(0.55f,k=>
            {
                transform.position=new Vector3(transform.position.x,Mathf.Lerp(-3.3f,-12f,k),0);
                transform.localScale=Vector3.one*Mathf.Lerp(1f,0.75f,k);
            },Easing.InCubic);
            gameObject.SetActive(false);
        }
    }
}
