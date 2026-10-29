using System;
using UnityEngine;

namespace NoriVerse.Gameplay.PizzaFinish
{
    /// <summary>Versioned completed-dish data, separate from an unfinished cooking attempt.</summary>
    public static class CompletedPizzaStore
    {
        private const string ProductionKey="NoriVerse.CompletedPizza.v1";
#if UNITY_EDITOR
        public static string EditorStorageKey { get; set; }
#endif
        public static string StorageKey
        {
            get
            {
#if UNITY_EDITOR
                if(!string.IsNullOrEmpty(EditorStorageKey))return EditorStorageKey;
#endif
                return ProductionKey;
            }
        }
        [Serializable]
        public sealed class Recipe
        {
            public int version=1;
            public PizzaPiece[] cheese,toppings;
        }
        public static void Save(Recipe recipe)
        {
            if(!Valid(recipe))return;
            PlayerPrefs.SetString(StorageKey,JsonUtility.ToJson(recipe));PlayerPrefs.Save();
        }
        public static bool TryLoad(out Recipe recipe)
        {
            recipe=null;string json=PlayerPrefs.GetString(StorageKey,"");
            if(string.IsNullOrEmpty(json)||json.Length>1000000)return false;
            try{recipe=JsonUtility.FromJson<Recipe>(json);}
            catch(ArgumentException){return false;}
            if(Valid(recipe))return true;
            recipe=null;return false;
        }
        private static bool Valid(Recipe recipe)
        {
            if(recipe==null||recipe.version!=1||recipe.cheese==null||recipe.toppings==null||recipe.cheese.Length==0||recipe.cheese.Length>512||recipe.toppings.Length>4096)return false;
            foreach(var piece in recipe.cheese)if(!Valid(piece))return false;
            foreach(var piece in recipe.toppings)if(!Valid(piece))return false;
            return true;
        }
        private static bool Valid(PizzaPiece piece)
        {
            return Finite(piece.position.x)&&Finite(piece.position.y)&&piece.position.sqrMagnitude<=16&&Finite(piece.rotation)&&Finite(piece.scale)&&piece.scale>=.1f&&piece.scale<=3f&&(int)piece.kind>=0&&(int)piece.kind<=3;
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
    }
}
