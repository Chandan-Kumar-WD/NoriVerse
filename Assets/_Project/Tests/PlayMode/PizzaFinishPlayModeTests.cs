using System.Collections;
using System.IO;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    public class PizzaFinishPlayModeTests
    {
        [SetUp]public void Setup(){PizzaTestProfile.Reset();Time.captureFramerate=60;PointerInput.ClearInjected();PizzaRecipeState.BeginNewRecipe();}
        [TearDown]public void Cleanup(){Time.captureFramerate=0;PointerInput.ClearInjected();PizzaRecipeState.BeginNewRecipe();}
        private static void Seed()
        {
            PizzaRecipeState.EnsurePreparedForDirectEntry();
            for(int i=0;i<4;i++)PizzaRecipeState.Toppings.Add(new PizzaPiece(new Vector2(i%2==0?-1.4f:1.4f,i<2?1.4f:-1.4f),i*20,1,(ToppingKind)i));
        }
        private static IEnumerator Finger(Vector2 from,Vector2 to,int frames=26)
        {
            PointerInput.InjectForTests(PointerPhase.Began,from);yield return null;
            for(int i=1;i<=frames;i++){PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(from,to,(float)i/frames));yield return null;}
            PointerInput.InjectForTests(PointerPhase.Ended,to);yield return null;PointerInput.ClearInjected();
        }
        private static void Fit(Camera cam,int w,int h)
        {
            cam.aspect=(float)w/h;cam.GetComponent<CameraFramingFitter>().Refit();
            foreach(var a in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))a.Refresh();
            foreach(var a in Object.FindObjectsByType<ScreenCoverFitter>(FindObjectsSortMode.None))a.Refit();
        }
        private static void Shot(string name,int width=1600,int height=728)
        {
            var cam=Camera.main;Fit(cam,width,height);
            var rt=new RenderTexture(width,height,24);var previous=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            string dir=Path.Combine(Application.dataPath,"../Docs/playmode_captures/pizza_upgrade/finish");Directory.CreateDirectory(dir);if(System.Environment.GetEnvironmentVariable("NORIVERSE_CAPTURE_FRAMES")=="1")File.WriteAllBytes(Path.Combine(dir,name+".png"),tex.EncodeToPNG());
            cam.targetTexture=null;RenderTexture.active=previous;cam.ResetAspect();cam.GetComponent<CameraFramingFitter>().Refit();
            foreach(var a in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))a.Refresh();
            foreach(var a in Object.FindObjectsByType<ScreenCoverFitter>(FindObjectsSortMode.None))a.Refit();
            Object.Destroy(tex);Object.Destroy(rt);
        }
        private static void AssertInside(Camera cam,Bounds bounds,string label)
        {
            var min=cam.WorldToViewportPoint(bounds.min);var max=cam.WorldToViewportPoint(bounds.max);
            Assert.That(min.x,Is.InRange(0f,1f),label+" left");Assert.That(min.y,Is.InRange(0f,1f),label+" bottom");
            Assert.That(max.x,Is.InRange(0f,1f),label+" right");Assert.That(max.y,Is.InRange(0f,1f),label+" top");
        }
        [UnityTest]
        public IEnumerator DecorationKeepsActualCheeseAndToppingsThroughOvenEntry()
        {
            PizzaRecipeState.EnsurePreparedForDirectEntry();int cheese=PizzaRecipeState.Cheese.Count;Vector2 first=PizzaRecipeState.Cheese[0].position;
            SceneManager.LoadScene("PizzaDecorate");yield return null;yield return new WaitForSeconds(0.8f);
            var game=Object.FindFirstObjectByType<PizzaDecorationController>();Shot("decoration_entry");
            yield return new WaitForSeconds(6.6f);Shot("decoration_hint");Assert.AreEqual(0,game.ToppingCount,"Ghost hint must not add actual toppings");
            yield return Finger(game.PalettePosition(ToppingKind.Tomato),new Vector2(-9,2));yield return new WaitForSeconds(0.45f);Assert.AreEqual(0,game.ToppingCount);
            for(int i=0;i<4;i++)
            {
                Vector2 local=new Vector2(i%2==0?-1.4f:1.4f,i<2?1.4f:-1.4f);
                yield return Finger(game.PalettePosition((ToppingKind)i),local+new Vector2(-0.85f,0));
                Assert.AreEqual(i+1,game.ToppingCount);Assert.Less(Vector2.Distance(local,PizzaRecipeState.Toppings[i].position),0.01f);
            }
            Shot("decorated_pizza");Assert.AreEqual(cheese,PizzaRecipeState.Cheese.Count);Assert.AreEqual(first,PizzaRecipeState.Cheese[0].position);
            yield return Finger(game.ConfirmPosition,game.ConfirmPosition,1);yield return new WaitForSeconds(2.2f);
            Assert.AreEqual("PizzaBake",SceneManager.GetActiveScene().name);
            var pizza=GameObject.Find("BakingPizza").GetComponent<PizzaVisual>();Assert.AreEqual(4,pizza.ToppingCount);Assert.AreEqual(cheese,pizza.CheeseCount);
            Assert.AreEqual(first,PizzaRecipeState.Cheese[0].position);Shot("oven_waiting");
        }
        [UnityTest]
        public IEnumerator OvenRequiresPowerAwardsThreeStarsAndReturnsToKitchen()
        {
            Seed();SceneManager.LoadScene("PizzaBake");yield return null;yield return new WaitForSeconds(1.6f);
            var oven=Object.FindFirstObjectByType<PizzaBakingController>();var reward=Object.FindFirstObjectByType<PizzaRewardController>();
            yield return new WaitForSeconds(4f);Shot("oven_power_hint");Assert.AreEqual(BakingPhase.Waiting,oven.Phase);Assert.AreEqual(0f,oven.Progress);Assert.IsFalse(PizzaRecipeState.IsBaked);
            bool completed=false;oven.OnDishCompleted.AddListener(()=>completed=true);
            yield return Finger(oven.PowerPosition,oven.PowerPosition,1);yield return new WaitForSeconds(2f);Assert.That(oven.Progress,Is.InRange(0.1f,0.6f));Shot("oven_half_baked");
            Assert.Greater(Object.FindFirstObjectByType<BakingProgressSparkles>().EmittedCount,3,"The progressing heat bar should emit sparkles");
            Assert.AreEqual(1f,GameObject.Find("BakingPizza").transform.Find("RawCrust").GetComponent<SpriteRenderer>().color.a,0.001f,"Browning must keep an opaque crust beneath the baked variant");
            float before=oven.Progress;yield return Finger(oven.PowerPosition,oven.PowerPosition,1);Assert.GreaterOrEqual(oven.Progress,before,"Repeated presses must not restart baking");
            yield return new WaitForSeconds(5.5f);Assert.IsTrue(PizzaRecipeState.IsBaked);Assert.AreEqual(BakingPhase.Reward,oven.Phase);Shot("reward_entry");
            yield return new WaitForSeconds(7.2f);Assert.AreEqual(3,reward.StarsAwarded);Shot("three_stars");
            foreach(var shape in new[]{("16x9",1920,1080),("4x3",1600,1200),("21x9",2100,900),("22x10",1600,728)})
            {
                var cam=Camera.main;Fit(cam,shape.Item2,shape.Item3);
                for(int i=0;i<3;i++)AssertInside(cam,GameObject.Find("RewardStar"+i).GetComponentInChildren<SpriteRenderer>().bounds,"reward star");
                AssertInside(cam,GameObject.Find("RewardNoriPortrait").GetComponent<SpriteRenderer>().bounds,"Nori portrait");
                Shot("reward_"+shape.Item1,shape.Item2,shape.Item3);
            }
            yield return new WaitForSeconds(1.8f);Shot("reward_confetti");
            yield return new WaitForSeconds(4f);Assert.IsTrue(completed);Assert.AreEqual("Kitchen",SceneManager.GetActiveScene().name);Assert.IsTrue(PizzaRecipeState.IsComplete);Shot("completed_kitchen");
        }
        [UnityTest]
        public IEnumerator CookedPizzaCanFeedNoriRepeatedlyAndMissesReturn()
        {
            Seed();PizzaRecipeState.IsBaked=true;SceneManager.LoadScene("Kitchen");yield return null;yield return new WaitForSeconds(1f);
            var serving=Object.FindFirstObjectByType<PizzaServingController>();DishButton dish=null;
            foreach(var d in Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None))if(!d.IsLocked)dish=d;
            Assert.IsNotNull(dish);Assert.IsNotNull(serving.ServedSprite);
            int opaque=0;foreach(var c in serving.ServedSprite.texture.GetPixels32())if(c.a>128)opaque++;
            Assert.Greater(opaque,20000,"The served pizza snapshot must actually contain the player's rendered meal");
            yield return new WaitForSeconds(3.4f);Shot("serving_hint");Assert.AreEqual(0,serving.Servings,"A ghost hint must not feed Nori");
            yield return Finger(dish.transform.position,new Vector2(-5,2));yield return new WaitForSeconds(0.6f);Assert.AreEqual(0,serving.Servings);Assert.IsFalse(serving.IsBusy);
            for(int i=1;i<=3;i++)
            {
                yield return Finger(dish.transform.position,serving.MouthPosition);
                yield return new WaitForSeconds(0.3f);if(i==1)Shot("nori_first_bite");
                yield return new WaitForSeconds(.8f);
                Assert.GreaterOrEqual(serving.BitesTaken,1,"An actual visible bite should be removed from the player's pizza");
                if(i==1)Shot("nori_bite_removed");
                yield return new WaitForSeconds(.7f);if(i==1)Shot("nori_chewing");
                yield return new WaitForSeconds(2.7f);Assert.AreEqual(i,serving.Servings);Assert.IsTrue(PizzaRecipeState.IsBaked);Assert.IsFalse(serving.IsBusy);
                Shot(i==3?"nori_sun_reaction":"nori_served_"+i);
            }
            Assert.AreEqual(4,PizzaRecipeState.Toppings.Count,"Repeated servings must preserve the completed recipe");
            yield return Finger(dish.transform.position,dish.transform.position,1);yield return new WaitForSeconds(2.7f);
            Assert.AreEqual("Cooking",SceneManager.GetActiveScene().name,"A short tap should still allow a fresh cooking run");Assert.IsFalse(PizzaRecipeState.IsBaked);
        }
        [UnityTest]
        public IEnumerator FinishingLayoutsAndCornerButtonsFitFourScreenShapes()
        {
            Seed();
            foreach(string scene in new[]{"PizzaDecorate","PizzaBake"})
            {
                SceneManager.LoadScene(scene);yield return null;yield return new WaitForSeconds(1.6f);
                var cam=Camera.main;
                foreach(var shape in new[]{("16x9",1920,1080),("4x3",1600,1200),("21x9",2100,900),("22x10",1600,728)})
                {
                    Fit(cam,shape.Item2,shape.Item3);
                    var focus=GameObject.Find(scene=="PizzaDecorate"?"PrepBoard":"OvenBody").GetComponent<SpriteRenderer>();AssertInside(cam,focus.bounds,scene);
                    if(scene=="PizzaDecorate")AssertInside(cam,GameObject.Find("ToppingPalette").GetComponent<SpriteRenderer>().bounds,"ingredient palette");
                    var button=GameObject.Find("BackButton").GetComponent<SpriteRenderer>();
                    Assert.That(cam.WorldToViewportPoint(button.bounds.min).x,Is.InRange(0f,0.05f));Assert.That(cam.WorldToViewportPoint(button.bounds.max).y,Is.InRange(0.95f,1f));
                    Shot(scene.ToLower()+"_"+shape.Item1,shape.Item2,shape.Item3);yield return null;
                }
            }
        }
    }
}
