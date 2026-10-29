using System.Collections;
using NoriVerse.Core;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEngine;
using NoriVerse.Audio;

namespace NoriVerse.Gameplay.PizzaFinish
{
    public class PizzaDecorationController:MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private PizzaVisual pizza;
        [SerializeField] private SpriteRenderer[] palette;
        [SerializeField] private Transform confirm;
        [SerializeField] private SpriteRenderer overlay;
        [SerializeField] private GestureHint hint;
        private SpriteRenderer dragging;
        private ToppingKind selected;
        private Vector3 home;
        private bool active,leaving;
        public int ToppingCount=>PizzaRecipeState.Toppings.Count;
        public Vector2 ConfirmPosition=>confirm.position;
        public Vector2 PalettePosition(ToppingKind kind)=>palette[(int)kind].transform.position;
        public void Configure(Camera cam,PizzaVisual meal,SpriteRenderer[] icons,Transform button,SpriteRenderer cover,GestureHint gesture)
        {sceneCamera=cam;pizza=meal;palette=icons;confirm=button;overlay=cover;hint=gesture;}
        private IEnumerator Start()
        {
            AudioDirector.Music(SoundId.MusGameplayLoop);
            PizzaRecipeState.EnsurePreparedForDirectEntry();pizza.Rebuild();pizza.SetBakedAmount(0);
            if(SceneTransition.IsBusy)Tween.SetAlpha(overlay,0);
            else{Tween.SetAlpha(overlay,1);yield return Tween.Fade(overlay,0,0.3f);}
            active=true;hint.ResetIdle();
        }
        private void Update()
        {
            if(!active || leaving || SceneTransition.IsBusy)return;
            var p=PointerInput.Read(sceneCamera);
            if(p.phase==PointerPhase.Began && dragging==null)
            {
                Vector2 toButton=p.world-(Vector2)confirm.position;
                if(Mathf.Abs(toButton.x)<1.15f && Mathf.Abs(toButton.y)<0.75f){leaving=true;hint.Stop();AudioDirector.Play(SoundId.ConfirmButton);SceneTransition.Go("PizzaBake");return;}
                for(int i=0;i<palette.Length;i++)
                    if(Vector2.Distance(p.world,palette[i].transform.position)<0.9f)
                    {
                        selected=(ToppingKind)i;home=palette[i].transform.position;
                        var go=new GameObject("Dragged_"+selected);go.transform.SetParent(transform,false);
                        dragging=go.AddComponent<SpriteRenderer>();dragging.sprite=palette[i].sprite;dragging.sortingOrder=130;dragging.transform.position=p.world;
                        hint.Stop();AudioDirector.Play(SoundId.ItemPickup);break;
                    }
            }
            if(dragging!=null && p.IsDown)
            {
                float dx=p.world.x-dragging.transform.position.x;
                dragging.transform.position=p.world;
                dragging.transform.rotation=Quaternion.Slerp(dragging.transform.rotation,Quaternion.Euler(0,0,Mathf.Clamp(-dx*15,-12,12)),0.4f);
            }
            if(dragging!=null && p.phase==PointerPhase.Ended)
            {
                var food=dragging;dragging=null;Vector2 local=pizza.transform.InverseTransformPoint(p.world);
                if(local.magnitude<3.45f)
                {
                    local=Vector2.ClampMagnitude(local,3.18f);
                    var piece=new PizzaPiece(local,food.transform.localEulerAngles.z,1f,selected);
                    AudioDirector.Play(SoundId.ToppingPlacePop,Random.Range(0.92f,1.1f));PizzaRecipeState.Toppings.Add(piece);pizza.AddTopping(piece);Destroy(food.gameObject);
                }
                else{AudioDirector.Play(SoundId.DropReturn);StartCoroutine(Return(food,home));}
            }
            if(dragging==null)hint.Offer(new[]{(Vector2)palette[(int)ToppingKind.Tomato].transform.position,(Vector2)pizza.transform.position+new Vector2(-1.8f,1.5f)},palette[(int)ToppingKind.Tomato].sprite,Vector3.one);
        }
        private IEnumerator Return(SpriteRenderer food,Vector3 destination)
        {yield return Tween.Move(food.transform,destination,0.35f,Easing.OutCubic);Destroy(food.gameObject);}
    }
}
