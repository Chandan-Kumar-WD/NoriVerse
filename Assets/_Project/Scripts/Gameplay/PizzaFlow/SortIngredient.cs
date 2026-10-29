using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFlow
{
    public enum ToppingKind{Tomato,Cheese,Salami,Basil}
    public class SortIngredient:MonoBehaviour
    {
        [SerializeField] private ToppingKind kind;
        [SerializeField] private SpriteRenderer render;
        public ToppingKind Kind=>kind;
        public SpriteRenderer Render=>render;
        public bool Available{get;set;}=true;
        public Vector3 Home{get;private set;}
        public Quaternion HomeRotation{get;private set;}
        public void Configure(ToppingKind value,SpriteRenderer sprite){kind=value;render=sprite;}
        private void Awake(){Home=transform.position;HomeRotation=transform.rotation;}
    }
}
