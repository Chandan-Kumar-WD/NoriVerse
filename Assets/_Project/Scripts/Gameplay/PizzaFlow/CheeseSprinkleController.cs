using NoriVerse.Core;
using NoriVerse.Utilities;
using NoriVerse.Gameplay.PizzaFinish;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public class CheeseSprinkleController : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private Sprite shred;
        [SerializeField] private Transform pieces;
        private bool active;
        private float nextPiece;
        public int Count{get;private set;}
        public bool CanConfirm=>Count>=3;
        public void Configure(Camera cam,Sprite source,Transform container){sceneCamera=cam;shred=source;pieces=container;}
        public void Begin(){active=true;}
        public void Stop(){active=false;}
        private void Update()
        {
            if(!active || SceneTransition.IsBusy || Count>=180)return;
            var p=PointerInput.Read(sceneCamera);
            if(!p.IsDown || Vector2.Distance(p.world,new Vector2(-0.85f,0))>3.15f || Time.time<nextPiece)return;
            nextPiece=Time.time+0.085f;
            var go=new GameObject("CheeseShred"+Count);go.transform.SetParent(pieces,false);
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=shred;sr.sortingOrder=35+Count%2;
            go.transform.position=p.world+Random.insideUnitCircle*0.09f;
            go.transform.rotation=Quaternion.Euler(0,0,Random.Range(0f,360f));
            go.transform.localScale=Vector3.one*Random.Range(1.15f,1.65f);
            PizzaRecipeState.Cheese.Add(new PizzaPiece((Vector2)go.transform.position-new Vector2(-0.85f,0),go.transform.localEulerAngles.z,go.transform.localScale.x));
            AudioDirector.Play(SoundId.CheeseSprinkle,Random.Range(0.9f,1.15f),0.14f);
            Count++;
        }
    }
}
