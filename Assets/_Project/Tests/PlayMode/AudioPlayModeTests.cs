using System;
using System.Collections;
using System.Collections.Generic;
using NoriVerse.Audio;
using NoriVerse.Gameplay.Cooking;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace NoriVerse.Tests
{
    /// <summary>Checks that each touch and game beat starts the sound the reference has for it (what plays, not how it sounds).</summary>
    public class AudioPlayModeTests
    {
        private readonly List<SoundId> heard = new List<SoundId>();

        [SetUp]
        public void Setup()
        {
            PizzaTestProfile.Reset();Time.captureFramerate=60;PointerInput.ClearInjected();PizzaRecipeState.BeginNewRecipe();
            heard.Clear();AudioDirector.Played+=Record;
        }
        [TearDown]
        public void Cleanup(){AudioDirector.Played-=Record;Time.captureFramerate=0;PointerInput.ClearInjected();PizzaRecipeState.BeginNewRecipe();}
        private void Record(SoundId id)=>heard.Add(id);
        private void AssertHeard(params SoundId[] ids){foreach(var id in ids)CollectionAssert.Contains(heard,id,$"Expected to hear {id}");}
        private int Count(SoundId id)=>heard.FindAll(x=>x==id).Count;
        private static IEnumerator Finger(Vector2 from,Vector2 to,int frames=24)
        {
            PointerInput.InjectForTests(PointerPhase.Began,from);yield return null;
            for(int i=1;i<=frames;i++){PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(from,to,(float)i/frames));yield return null;}
            PointerInput.InjectForTests(PointerPhase.Ended,to);yield return null;PointerInput.ClearInjected();
        }

        [Test]
        public void LibraryHasAClipForEverySound()
        {
            var library=Resources.Load<AudioLibrary>(AudioLibrary.ResourcePath);
            Assert.IsNotNull(library,"Run NoriVerse/Build Audio Library");
            var found=new HashSet<SoundId>();
            foreach(var e in library.Entries){Assert.IsNotNull(e.clip,$"{e.id} has no clip");Assert.Greater(e.volume,0f);found.Add(e.id);}
            foreach(SoundId id in Enum.GetValues(typeof(SoundId)))if(id!=SoundId.None)Assert.IsTrue(found.Contains(id),$"No clip for {id}");
        }

        [UnityTest]
        public IEnumerator SplashPlaysJingleAndNoriCallsTheName()
        {
            SceneManager.LoadScene("Splash");yield return null;yield return new WaitForSeconds(1.3f);
            AssertHeard(SoundId.MusSplash,SoundId.VoBrand);
            Assert.AreEqual(1,Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,"The splash needs an AudioListener too");
            yield return new WaitForSeconds(5.4f);AssertHeard(SoundId.VoNom);
        }

        [UnityTest]
        public IEnumerator KitchenLoopAndCookingTouchesHaveSounds()
        {
            SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(2.2f);
            Assert.AreEqual(SoundId.MusGameplayLoop,AudioDirector.CurrentMusic);
            int ears=0;foreach(var l in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))if(l.isActiveAndEnabled)ears++;
            Assert.AreEqual(1,ears,"Exactly one active AudioListener is needed for anything to be audible");
            DishButton locked=null;foreach(var d in Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None))if(d.IsLocked)locked=d;
            Object.FindFirstObjectByType<KitchenMenuController>().TapDish(locked);yield return null;AssertHeard(SoundId.DishLockedWobble);

            heard.Clear();SceneManager.LoadScene("Cooking");yield return null;yield return new WaitForSeconds(1.3f);
            Assert.GreaterOrEqual(Count(SoundId.ItemPopIn),3,"Ingredients pop into the tray with a sound");
            var drag=Object.FindFirstObjectByType<IngredientDragController>();var bowl=Object.FindFirstObjectByType<BowlArea>();
            var first=drag.Ingredients[0];var second=drag.Ingredients[1];
            yield return Finger(first.transform.position,bowl.transform.position);yield return new WaitForSeconds(.4f);
            AssertHeard(SoundId.ItemPickup,SoundId.DropPlopBowl);
            yield return Finger(second.transform.position,new Vector2(-6,3.5f));yield return new WaitForSeconds(.4f);
            AssertHeard(SoundId.DropReturn);
            Assert.AreEqual(SoundId.MusGameplayLoop,AudioDirector.CurrentMusic,"Music continues from the kitchen");
        }

        [UnityTest]
        public IEnumerator RollingHasALoopThatStopsWhenTheFingerStops()
        {
            SceneManager.LoadScene("Dough");yield return null;yield return new WaitForSeconds(1.6f);
            var rolling=Object.FindFirstObjectByType<DoughRollingController>();Assert.IsTrue(rolling.IsActive);
            Vector2 a=new Vector2(-2,0),b=new Vector2(2,0);
            PointerInput.InjectForTests(PointerPhase.Began,Vector2.zero);yield return null;
            for(int i=0;i<40;i++){PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(a,b,Mathf.PingPong(i/10f,1)));yield return null;}
            Assert.IsTrue(AudioDirector.IsSustaining(SoundId.RollingPinLoop),"Rolling should sound while the pin moves");
            PointerInput.InjectForTests(PointerPhase.Ended,b);yield return null;PointerInput.ClearInjected();
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsFalse(AudioDirector.IsSustaining(SoundId.RollingPinLoop),"Rolling sound fades once the finger stops");
        }

        [UnityTest]
        public IEnumerator OvenBakeAndRewardBeatsHaveSounds()
        {
            PizzaRecipeState.EnsurePreparedForDirectEntry();
            SceneManager.LoadScene("PizzaBake");yield return null;yield return new WaitForSeconds(1.8f);
            AssertHeard(SoundId.OvenRackSlide);
            var oven=Object.FindFirstObjectByType<PizzaBakingController>();
            yield return Finger(oven.PowerPosition,oven.PowerPosition,1);yield return new WaitForSeconds(1f);
            AssertHeard(SoundId.OvenPowerClick);Assert.IsTrue(AudioDirector.IsSustaining(SoundId.OvenBakeLoop));
            yield return new WaitForSeconds(6.5f);AssertHeard(SoundId.OvenDoneDing);
            yield return new WaitForSeconds(9f);
            AssertHeard(SoundId.SuccessArpeggio,SoundId.VoWow,SoundId.ConfettiPop);
            Assert.AreEqual(3,Count(SoundId.StarDing),"One ding per star");
        }

        [UnityTest]
        public IEnumerator FeedingNoriPlaysAnticipationBitesChewAndYummyInOrder()
        {
            PizzaRecipeState.EnsurePreparedForDirectEntry();PizzaRecipeState.IsBaked=true;
            SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(1f);
            var serving=Object.FindFirstObjectByType<PizzaServingController>();DishButton dish=null;
            foreach(var d in Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None))if(!d.IsLocked)dish=d;
            heard.Clear();yield return Finger(dish.transform.position,serving.MouthPosition);yield return new WaitForSeconds(4.5f);
            int[] order={heard.IndexOf(SoundId.ItemPickup),heard.IndexOf(SoundId.VoMmmAnticipate),heard.IndexOf(SoundId.VoNom),heard.IndexOf(SoundId.VoChew),heard.IndexOf(SoundId.VoYummy)};
            foreach(int i in order)Assert.GreaterOrEqual(i,0,"Missing a feeding sound: "+string.Join(",",heard));
            for(int i=1;i<order.Length;i++)Assert.Greater(order[i],order[i-1],"Feeding sounds out of order: "+string.Join(",",heard));
        }
    }
}
