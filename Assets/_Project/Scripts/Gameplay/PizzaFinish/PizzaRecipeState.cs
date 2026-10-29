using System;
using System.Collections.Generic;
using NoriVerse.Gameplay.PizzaFlow;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    [Serializable]
    public struct PizzaPiece
    {
        public Vector2 position;
        public float rotation,scale;
        public ToppingKind kind;
        public PizzaPiece(Vector2 p,float angle,float size,ToppingKind type=ToppingKind.Cheese)
        {position=p;rotation=angle;scale=size;kind=type;}
    }
    /// <summary>Current cooking state plus a separately saved completed pizza, retained across sessions.</summary>
    public static class PizzaRecipeState
    {
        public static readonly List<PizzaPiece> Cheese=new List<PizzaPiece>();
        public static readonly List<PizzaPiece> Toppings=new List<PizzaPiece>();
        public static bool IsPrepared{get;set;}
        public static bool IsBaked{get;set;}
        public static bool HasReturnedToKitchen{get;set;}
        public static int Servings{get;set;}
        public static bool IsComplete=>IsBaked;
        private static CompletedPizzaStore.Recipe completed;
        private static bool completionHintPending;
        public static bool HasCompletedDish=>completed!=null||IsBaked;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void InitializeSession()
        {
            completed=null;BeginNewRecipe();
            if(CompletedPizzaStore.TryLoad(out var saved)){completed=saved;RestoreCompletedForHome();}
        }
        public static void MarkCompleted()
        {
            IsBaked=true;completionHintPending=true;
            completed=new CompletedPizzaStore.Recipe{cheese=Cheese.ToArray(),toppings=Toppings.ToArray()};
            CompletedPizzaStore.Save(completed);
        }
        public static bool ConsumeCompletionHint()
        {bool pending=completionHintPending;completionHintPending=false;return pending;}
        public static void RestoreCompletedForHome()
        {
            if(IsBaked||completed==null)return;
            Cheese.Clear();Cheese.AddRange(completed.cheese);Toppings.Clear();Toppings.AddRange(completed.toppings);
            IsPrepared=true;IsBaked=true;
        }
        public static void BeginNewRecipe()
        {Cheese.Clear();Toppings.Clear();IsPrepared=false;IsBaked=false;HasReturnedToKitchen=false;Servings=0;completionHintPending=false;}
        public static void EnsurePreparedForDirectEntry()
        {
            // Opening a later scene directly in the editor needs an initial cheese pizza.
            // Normal play already has the user's exact cheese positions and never replaces them.
            if(IsPrepared || Cheese.Count>0){IsPrepared=true;return;}
            for(int i=0;i<34;i++)
            {
                float a=i*2.399963f,r=0.45f+2.35f*Mathf.Sqrt((i+0.5f)/34f);
                Cheese.Add(new PizzaPiece(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,i*47f,1f));
            }
            IsPrepared=true;
        }
    }
}
