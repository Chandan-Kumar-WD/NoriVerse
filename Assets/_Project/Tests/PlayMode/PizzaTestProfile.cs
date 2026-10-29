using System;
using NoriVerse.Gameplay.PizzaFinish;
using NUnit.Framework;
using UnityEngine;

namespace NoriVerse.Tests
{
    /// <summary>Tests use a separate preference key and cannot overwrite the player's completed dish.</summary>
    [SetUpFixture]
    public class PizzaTestProfile
    {
        [OneTimeSetUp] public void Begin()=>CompletedPizzaStore.EditorStorageKey="NoriVerse.Tests.CompletedPizza."+Guid.NewGuid().ToString("N");
        [OneTimeTearDown] public void End()
        {
            PlayerPrefs.DeleteKey(CompletedPizzaStore.StorageKey);PlayerPrefs.Save();
            CompletedPizzaStore.EditorStorageKey=null;PizzaRecipeState.InitializeSession();
        }
        public static void Reset()
        {PlayerPrefs.DeleteKey(CompletedPizzaStore.StorageKey);PizzaRecipeState.InitializeSession();}
    }
}
