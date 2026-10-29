using System.Collections;
using System.IO;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    public class PizzaFlowPlayModeTests
    {
        [SetUp] public void Setup(){PizzaTestProfile.Reset();Time.captureFramerate=60;PointerInput.ClearInjected();PizzaRecipeState.BeginNewRecipe();}
        [TearDown] public void Teardown(){Time.captureFramerate=0;PointerInput.ClearInjected();}
        private static void Shot(string name,int width=1600,int height=728)
        {
            var cam=Camera.main;
            cam.aspect=(float)width/height;cam.GetComponent<CameraFramingFitter>().Refit();
            foreach(var a in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))a.Refresh();
            var rt=new RenderTexture(width,height,24);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            string dir=Path.Combine(Application.dataPath,"../Docs/playmode_captures/pizza_upgrade/prep");Directory.CreateDirectory(dir);
            if(System.Environment.GetEnvironmentVariable("NORIVERSE_CAPTURE_FRAMES")=="1")File.WriteAllBytes(Path.Combine(dir,name+".png"),tex.EncodeToPNG());
            cam.targetTexture=null;RenderTexture.active=old;cam.ResetAspect();cam.GetComponent<CameraFramingFitter>().Refit();
            Object.Destroy(tex);Object.Destroy(rt);
        }
        private static IEnumerator Finger(Vector2 from,Vector2 to,int frames=24)
        {
            PointerInput.InjectForTests(PointerPhase.Began,from);yield return null;
            for(int i=1;i<=frames;i++){PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(from,to,(float)i/frames));yield return null;}
            PointerInput.InjectForTests(PointerPhase.Ended,to);yield return null;PointerInput.ClearInjected();
        }
        [UnityTest]
        public IEnumerator DoughRollAndOrderedCutLeadToPizzaPreparation()
        {
            SceneManager.LoadScene("Dough");yield return null;yield return new WaitForSeconds(1.8f);
            var roll=Object.FindFirstObjectByType<DoughRollingController>();var cut=Object.FindFirstObjectByType<DoughCutController>();
            Assert.IsTrue(roll.IsActive);Shot("dough_entry");
            yield return new WaitForSeconds(6.7f);Shot("rolling_hint");Assert.AreEqual(0f,roll.Progress,"Hints must never roll the real dough");
            var pin=GameObject.Find("RollingPin").transform;
            PointerInput.InjectForTests(PointerPhase.Began,pin.position);yield return null;
            for(int i=0;i<40;i++){PointerInput.InjectForTests(PointerPhase.Held,pin.position);yield return null;}
            Assert.AreEqual(0f,roll.Progress,0.001f,"Holding still must not roll dough");
            Vector2 previous=pin.position;
            bool halfShot=false;
            for(int leg=0;leg<24 && roll.Progress<1;leg++)
            {
                Vector2 target=new Vector2(leg%2==0?-4.2f:4.2f,leg%3==0?2.6f:-2.6f);
                for(int i=1;i<=32 && roll.Progress<1;i++)
                {
                    PointerInput.InjectForTests(PointerPhase.Held,Vector2.Lerp(previous,target,(float)i/32));yield return null;
                    if(roll.IsActive)
                    {
                        var b=pin.GetComponent<SpriteRenderer>().bounds;var camera=Camera.main;
                        Assert.GreaterOrEqual(camera.WorldToViewportPoint(b.min).y,0f,"Rolling pin must stay fully visible while turning");
                        Assert.LessOrEqual(camera.WorldToViewportPoint(b.max).y,1f,"Rolling pin must stay fully visible while turning");
                    }
                    if(!halfShot && roll.Progress>0.5f){Shot("dough_half_rolled");halfShot=true;}
                }
                previous=target;
            }
            PointerInput.InjectForTests(PointerPhase.Ended,previous);yield return null;PointerInput.ClearInjected();
            Assert.AreEqual(1f,roll.Progress,0.001f);yield return new WaitForSeconds(1f);Assert.IsTrue(cut.IsActive);Shot("cut_guide");
            Vector2 start=cut.MarkerPosition;
            PointerInput.InjectForTests(PointerPhase.Began,start);yield return null;
            float a=(DoughCutController.StartAngle+25)*Mathf.Deg2Rad;
            PointerInput.InjectForTests(PointerPhase.Held,DoughCutController.Center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*DoughCutController.Radius);yield return null;
            Assert.AreEqual(0f,cut.Progress,0.001f,"Tracing backwards must not advance the cut");
            PointerInput.InjectForTests(PointerPhase.Held,DoughCutController.Center-(start-DoughCutController.Center));yield return null;
            Assert.AreEqual(0f,cut.Progress,0.001f,"Jumping across the circle must not advance the cut");
            PointerInput.InjectForTests(PointerPhase.Ended,start);yield return null;
            PointerInput.InjectForTests(PointerPhase.Began,start);yield return null;
            for(int i=1;i<=180 && cut.Progress<1;i++)
            {
                a=(DoughCutController.StartAngle-i*2f)*Mathf.Deg2Rad;
                PointerInput.InjectForTests(PointerPhase.Held,DoughCutController.Center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*DoughCutController.Radius);yield return null;
                if(i==90)Shot("cut_half");
            }
            PointerInput.ClearInjected();Assert.AreEqual(1f,cut.Progress,0.001f);
            yield return new WaitForSeconds(0.8f);Shot("cut_round_base");
            yield return new WaitForSeconds(3f);Assert.AreEqual("PizzaPrep",SceneManager.GetActiveScene().name);
        }
        [UnityTest]
        public IEnumerator SauceCountsUniqueCoverageAndCheeseRequiresConfirmation()
        {
            SceneManager.LoadScene("PizzaPrep");yield return null;yield return new WaitForSeconds(0.8f);
            var paint=Object.FindFirstObjectByType<SaucePaintController>();var prep=Object.FindFirstObjectByType<PizzaPrepController>();var cheese=Object.FindFirstObjectByType<CheeseSprinkleController>();
            Shot("pizza_plain");yield return new WaitForSeconds(6.6f);Shot("sauce_hint");Assert.AreEqual(0f,paint.Coverage);
            Vector2 center=new Vector2(-0.85f,0);
            PointerInput.InjectForTests(PointerPhase.Began,center);yield return null;
            PointerInput.InjectForTests(PointerPhase.Held,center);yield return null;
            float first=paint.Coverage;
            for(int i=0;i<40;i++){PointerInput.InjectForTests(PointerPhase.Held,center);yield return null;}
            Assert.AreEqual(first,paint.Coverage,0.001f,"Painting the same spot must not inflate coverage");Shot("sauce_first_stroke");
            for(int row=0;row<=14 && paint.IsActive;row++)
            {
                float y=-3.3f+row*0.47f;
                for(int col=0;col<=14 && paint.IsActive;col++)
                {
                    float x=-3.3f+(row%2==0?col:14-col)*0.47f;
                    PointerInput.InjectForTests(PointerPhase.Held,center+new Vector2(x,y));yield return null;
                }
                if(row==6)Shot("sauce_half");
            }
            PointerInput.InjectForTests(PointerPhase.Ended,center);yield return null;PointerInput.ClearInjected();
            Assert.GreaterOrEqual(paint.Coverage,0.9f);yield return new WaitForSeconds(2.2f);
            Assert.AreEqual(PizzaPrepPhase.Cheese,prep.Phase);Shot("sauce_complete");
            yield return Finger(prep.ConfirmPosition,prep.ConfirmPosition,1);
            Assert.AreEqual(PizzaPrepPhase.Cheese,prep.Phase,"An empty pizza cannot be confirmed before adding cheese");
            Assert.AreEqual(0,cheese.Count,"Touching outside the pizza must not sprinkle cheese");
            for(int i=0;i<75;i++)
            {
                float angle=i*0.63f;float r=0.4f+(i%18)*0.14f;
                Vector2 p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;
                PointerInput.InjectForTests(i==0?PointerPhase.Began:PointerPhase.Held,p);
                yield return new WaitForSeconds(0.09f);
            }
            PointerInput.InjectForTests(PointerPhase.Ended,center);yield return null;PointerInput.ClearInjected();
            Assert.IsTrue(cheese.CanConfirm);Assert.AreEqual("PizzaPrep",SceneManager.GetActiveScene().name,"Adding cheese must wait for explicit confirmation");Shot("cheese_ready");
            yield return Finger(prep.ConfirmPosition,prep.ConfirmPosition,1);
            yield return new WaitForSeconds(2f);Assert.AreEqual("ToppingSort",SceneManager.GetActiveScene().name);
        }
        private static SortIngredient Pick(ToppingSortController game,ToppingKind kind)
        {
            SortIngredient chosen=null;
            foreach(var item in game.Ingredients)if(item.Available && item.Kind==kind && (chosen==null || item.Render.sortingOrder>chosen.Render.sortingOrder))chosen=item;
            return chosen;
        }
        private static SortTarget Target(ToppingSortController game,ToppingKind kind)
        {foreach(var t in game.Targets)if(t.Kind==kind)return t;return null;}
        [UnityTest]
        public IEnumerator SortingRejectsWrongDropsAndFinishesEachCategoryOnce()
        {
            SceneManager.LoadScene("ToppingSort");yield return null;yield return new WaitForSeconds(1.6f);
            var game=Object.FindFirstObjectByType<ToppingSortController>();Shot("sorting_entry");
            yield return new WaitForSeconds(6.5f);Shot("sorting_hint");Assert.AreEqual(0,game.CompletedCount,"A ghost hint must not consume any toppings");
            var wrong=Pick(game,ToppingKind.Salami);Vector3 home=wrong.Home;
            yield return Finger(wrong.transform.position,Target(game,ToppingKind.Tomato).DropPoint);
            yield return new WaitForSeconds(0.6f);Assert.IsTrue(wrong.Available);Assert.AreEqual(0,game.CompletedCount);Assert.Less(Vector3.Distance(home,wrong.transform.position),0.01f);
            bool completed=false;game.OnCompleted.AddListener(()=>completed=true);
            var order=new[]{ToppingKind.Salami,ToppingKind.Cheese,ToppingKind.Basil,ToppingKind.Tomato};
            for(int i=0;i<order.Length;i++)
            {
                var item=Pick(game,order[i]);var target=Target(game,order[i]);
                yield return Finger(item.transform.position,target.DropPoint);
                yield return new WaitForSeconds(0.4f);Assert.IsTrue(target.IsCompleted,"Correct topping should fill its matching plate");
                Assert.AreEqual(i+1,game.CompletedCount);Shot("sorting_correct_"+order[i]);
                yield return new WaitForSeconds(1.5f);
            }
            Assert.IsTrue(game.IsComplete);yield return new WaitForSeconds(0.8f);Assert.IsTrue(completed);Shot("sorting_finished");
            yield return new WaitForSeconds(1.3f);Assert.AreEqual("PizzaDecorate",SceneManager.GetActiveScene().name);
        }
        [UnityTest]
        public IEnumerator NewBackButtonReturnsToKitchen()
        {
            SceneManager.LoadScene("Dough");yield return null;yield return new WaitForSeconds(1.7f);
            var button=GameObject.Find("BackButton").transform;
            yield return Finger(button.position,button.position,1);
            Assert.IsTrue(ExitConfirmation.IsOpen);
            var dialog=Object.FindFirstObjectByType<ExitConfirmation>();
            yield return Finger(dialog.ConfirmPosition,dialog.ConfirmPosition,1);
            yield return new WaitForSeconds(1.8f);
            Assert.AreEqual("Kitchen",SceneManager.GetActiveScene().name);
        }
        [UnityTest]
        public IEnumerator NewScenesFitFourAspectRatiosWithSafeCornerButtons()
        {
            var shapes=new[]{("16x9",1920,1080),("4x3",1600,1200),("21x9",2100,900),("22x10",1600,728)};
            foreach(string scene in new[]{"Dough","PizzaPrep","ToppingSort"})
            {
                SceneManager.LoadScene(scene);yield return null;yield return new WaitForSeconds(1.6f);
                var cam=Camera.main;
                string focus=scene=="Dough"?"RollingBoard":scene=="PizzaPrep"?"CreamPrepBoard":"ToppingTray";
                foreach(var shape in shapes)
                {
                    cam.aspect=(float)shape.Item2/shape.Item3;cam.GetComponent<CameraFramingFitter>().Refit();
                    foreach(var a in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))a.Refresh();
                    var b=GameObject.Find(focus).GetComponent<SpriteRenderer>().bounds;
                    Vector3 min=cam.WorldToViewportPoint(b.min),max=cam.WorldToViewportPoint(b.max);
                    Assert.That(min.x,Is.InRange(0f,1f));Assert.That(min.y,Is.InRange(0f,1f));Assert.That(max.x,Is.InRange(0f,1f));Assert.That(max.y,Is.InRange(0f,1f));
                    var back=GameObject.Find("BackButton").GetComponent<SpriteRenderer>();
                    Assert.That(cam.WorldToViewportPoint(back.bounds.min).x,Is.InRange(0f,0.05f));
                    Assert.That(cam.WorldToViewportPoint(back.bounds.max).y,Is.InRange(0.95f,1f));
                    Shot(scene.ToLower()+"_"+shape.Item1,shape.Item2,shape.Item3);
                    yield return null;
                }
            }
        }
    }
}
