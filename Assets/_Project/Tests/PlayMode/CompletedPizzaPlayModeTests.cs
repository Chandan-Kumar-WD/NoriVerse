using System.Collections;
using NoriVerse.Characters;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    public class CompletedPizzaPlayModeTests
    {
        [SetUp] public void Setup(){PizzaTestProfile.Reset();Time.captureFramerate=60;PointerInput.ClearInjected();}
        [TearDown] public void Cleanup(){Time.captureFramerate=0;PointerInput.ClearInjected();}
        private static void Complete()
        {
            PizzaRecipeState.EnsurePreparedForDirectEntry();
            PizzaRecipeState.Toppings.Add(new PizzaPiece(new Vector2(.7f,.5f),27,1.1f,ToppingKind.Basil));
            PizzaRecipeState.MarkCompleted();
        }
        private static IEnumerator Tap(Vector2 position)
        {
            PointerInput.InjectForTests(PointerPhase.Began,position);yield return null;
            PointerInput.InjectForTests(PointerPhase.Ended,position);yield return null;PointerInput.ClearInjected();yield return null;
        }
        [Test] public void CompletedRecipeSurvivesColdStartAndUnfinishedReplay()
        {
            Complete();Vector2 cheese=PizzaRecipeState.Cheese[0].position;
            PizzaRecipeState.InitializeSession();
            Assert.IsTrue(PizzaRecipeState.IsBaked);Assert.AreEqual(34,PizzaRecipeState.Cheese.Count);Assert.AreEqual(cheese,PizzaRecipeState.Cheese[0].position);
            Assert.AreEqual(new Vector2(.7f,.5f),PizzaRecipeState.Toppings[0].position);Assert.IsFalse(PizzaRecipeState.ConsumeCompletionHint(),"Cold launch must not claim a new cooking completion");
            PizzaRecipeState.BeginNewRecipe();Assert.IsFalse(PizzaRecipeState.IsBaked);Assert.IsTrue(PizzaRecipeState.HasCompletedDish);
            PizzaRecipeState.Cheese.Add(new PizzaPiece(Vector2.zero,0,1));
            PizzaRecipeState.RestoreCompletedForHome();Assert.IsTrue(PizzaRecipeState.IsBaked);Assert.AreEqual(34,PizzaRecipeState.Cheese.Count);Assert.AreEqual(1,PizzaRecipeState.Toppings.Count);
        }
        [Test] public void InvalidSavedRecipeDoesNotUnlockFeeding()
        {
            PlayerPrefs.SetString(CompletedPizzaStore.StorageKey,"{broken");PizzaRecipeState.InitializeSession();Assert.IsFalse(PizzaRecipeState.IsBaked);
            PlayerPrefs.SetString(CompletedPizzaStore.StorageKey,"{\"version\":99,\"cheese\":[],\"toppings\":[]}");PizzaRecipeState.InitializeSession();Assert.IsFalse(PizzaRecipeState.HasCompletedDish);
        }
        [UnityTest] public IEnumerator GuideNeedsCompletionAndRespectsActivityAndIdle()
        {
            SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(16f);
            var serving=Object.FindFirstObjectByType<PizzaServingController>();Assert.IsFalse(serving.IsFeedingGuidePlaying,"No dish completed: no feeding guide even after inactivity");Assert.IsNull(serving.ServedSprite);
            Complete();SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(1.4f);
            serving=Object.FindFirstObjectByType<PizzaServingController>();Assert.IsTrue(serving.IsFeedingGuidePlaying,"Newly completed cooking gets one initial demonstration");
            yield return Tap(new Vector2(-8,3));Assert.IsFalse(serving.IsFeedingGuidePlaying);
            for(int i=0;i<14;i++)
            {yield return Tap(new Vector2(-8,3));yield return new WaitForSeconds(.9f);Assert.IsFalse(serving.IsFeedingGuidePlaying,"Active players should not see a feeding guide");}
            yield return new WaitForSeconds(12.3f);Assert.IsTrue(serving.IsFeedingGuidePlaying,"Idle completed-dish owners can receive a reminder");
            yield return Tap(new Vector2(-8,3));Assert.IsFalse(serving.IsFeedingGuidePlaying);
        }
        [UnityTest] public IEnumerator SavedDishCanBeFedImmediatelyAfterRestart()
        {
            Complete();PizzaRecipeState.InitializeSession();SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(1f);
            var serving=Object.FindFirstObjectByType<PizzaServingController>();DishButton pizza=null;
            foreach(var dish in Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None))if(!dish.IsLocked)pizza=dish;
            Assert.IsNotNull(serving.ServedSprite);Assert.IsFalse(serving.IsFeedingGuidePlaying,"A normal home launch is not a new completion");
            PointerInput.InjectForTests(PointerPhase.Began,pizza.transform.position);yield return null;
            for(int i=1;i<=24;i++){PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(pizza.transform.position,serving.MouthPosition,i/24f));yield return null;}
            PointerInput.InjectForTests(PointerPhase.Ended,serving.MouthPosition);yield return null;PointerInput.ClearInjected();
            yield return new WaitForSeconds(4.5f);Assert.AreEqual(1,serving.Servings);Assert.IsTrue(PizzaRecipeState.HasCompletedDish);Assert.IsFalse(serving.IsFeedingGuidePlaying);
        }
        [UnityTest] public IEnumerator AwayExpressionsHoldAndTouchReturnsToHappy()
        {
            SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(29f);
            var mood=Object.FindFirstObjectByType<NoriMoodDirector>();var face=GameObject.Find("NoriBust").GetComponent<SpriteCrossfader>();
            Assert.IsTrue(mood.IsSad);Assert.AreEqual(NoriMoodDirector.Worried,face.Current);
            yield return new WaitForSeconds(7f);Assert.AreEqual(NoriMoodDirector.Worried,face.Current,"A subdued face should hold rather than cycle through many emotions");
            yield return new WaitForSeconds(6f);Assert.AreEqual(NoriMoodDirector.Sleepy,face.Current);
            yield return Tap(new Vector2(-8,3));Assert.IsFalse(mood.IsSad);Assert.AreEqual(NoriMoodDirector.BigSmile,face.Current);
            yield return new WaitForSeconds(1.5f);Assert.AreEqual(NoriMoodDirector.BigSmile,face.Current,"Cheering should hold a happy expression");
        }
    }
}
