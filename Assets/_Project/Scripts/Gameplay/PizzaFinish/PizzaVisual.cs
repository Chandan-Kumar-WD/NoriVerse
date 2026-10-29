using System.Collections.Generic;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>One coherent layered rendering of the custom pizza, reused at every stage and size.</summary>
    public class PizzaVisual:MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rawCrust,bakedCrust,sauce;
        [SerializeField] private Transform cheeseContainer,toppingContainer;
        [SerializeField] private Sprite cheeseSprite;
        [SerializeField] private Sprite[] toppingSprites;
        [SerializeField] private int order=20;
        private readonly List<SpriteRenderer> cheese=new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> toppings=new List<SpriteRenderer>();
        private float baked,opacity=1;
        public int CheeseCount=>cheese.Count;
        public int ToppingCount=>toppings.Count;
        public void Configure(SpriteRenderer raw,SpriteRenderer cooked,SpriteRenderer red,Transform shreds,Transform food,Sprite shred,Sprite[] types,int sortingOrder)
        {rawCrust=raw;bakedCrust=cooked;sauce=red;cheeseContainer=shreds;toppingContainer=food;cheeseSprite=shred;toppingSprites=types;order=sortingOrder;}
        private void Awake(){Rebuild();SetBakedAmount(PizzaRecipeState.IsBaked?1:0);}
        public void Rebuild()
        {
            Clear(cheeseContainer);Clear(toppingContainer);cheese.Clear();toppings.Clear();
            foreach(var p in PizzaRecipeState.Cheese)cheese.Add(Piece(cheeseContainer,"Cheese",cheeseSprite,p,order+3));
            foreach(var p in PizzaRecipeState.Toppings)AddTopping(p);
            Apply();
        }
        private static void Clear(Transform parent)
        {
            for(int i=parent.childCount-1;i>=0;i--)
            {var child=parent.GetChild(i);child.gameObject.SetActive(false);Destroy(child.gameObject);}
        }
        private static SpriteRenderer Piece(Transform parent,string name,Sprite source,PizzaPiece data,int sortingOrder)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            go.transform.localPosition=data.position;go.transform.localRotation=Quaternion.Euler(0,0,data.rotation);go.transform.localScale=Vector3.one*data.scale;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=source;sr.sortingOrder=sortingOrder;return sr;
        }
        public void AddTopping(PizzaPiece data)
        {toppings.Add(Piece(toppingContainer,"Topping_"+data.kind,toppingSprites[(int)data.kind],data,order+5+toppings.Count%2));Apply();}
        public void SetBakedAmount(float value){baked=Mathf.Clamp01(value);Apply();}
        public void SetOpacity(float value){opacity=Mathf.Clamp01(value);Apply();}
        private void Apply()
        {
            if(rawCrust==null)return;
            // Keep the raw crust opaque beneath the incoming baked texture, so
            // intermediate browning never exposes the wire rack through the food.
            Tween.SetAlpha(rawCrust,opacity);Tween.SetAlpha(bakedCrust,baked*opacity);Tween.SetAlpha(sauce,opacity);
            foreach(var sr in cheese)sr.color=new Color(1,Mathf.Lerp(1,0.95f,baked),Mathf.Lerp(1,0.85f,baked),opacity);
            foreach(var sr in toppings)sr.color=new Color(1,Mathf.Lerp(1,0.97f,baked),Mathf.Lerp(1,0.93f,baked),opacity);
        }
    }
}
