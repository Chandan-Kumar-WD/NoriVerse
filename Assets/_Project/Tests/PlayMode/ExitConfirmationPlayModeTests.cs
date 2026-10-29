using System.Collections;
using System.IO;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    public class ExitConfirmationPlayModeTests
    {
        [SetUp] public void Setup(){PizzaTestProfile.Reset();Time.timeScale=1;PointerInput.ClearInjected();}
        [TearDown] public void Teardown()
        {
            var modal=Object.FindFirstObjectByType<ExitConfirmation>();if(modal!=null)modal.Cancel();
            Time.timeScale=1;PointerInput.ClearInjected();
        }
        private static IEnumerator Tap(Vector2 point)
        {
            PointerInput.InjectForTests(PointerPhase.Began,point);yield return null;
            PointerInput.InjectForTests(PointerPhase.Ended,point);yield return null;
            PointerInput.ClearInjected();yield return null;
        }
        private static void Capture(string name)
        {
            var cam=Camera.main;cam.aspect=1600f/900;cam.GetComponent<CameraFramingFitter>().Refit();
            foreach(var anchor in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))anchor.Refresh();
            Object.FindFirstObjectByType<ExitConfirmation>().Refit();
            var rt=new RenderTexture(1600,900,24);var old=RenderTexture.active;
            cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
            string path=Path.Combine(Application.dataPath,"../Docs/playmode_captures/exit_confirmation");Directory.CreateDirectory(path);if(System.Environment.GetEnvironmentVariable("NORIVERSE_CAPTURE_FRAMES")=="1")File.WriteAllBytes(Path.Combine(path,name+".png"),tex.EncodeToPNG());
            cam.targetTexture=null;RenderTexture.active=old;Object.Destroy(rt);Object.Destroy(tex);
        }
        [UnityTest] public IEnumerator AllGameplayScreensPauseCancelCloseAndConfirmHome()
        {
            foreach(string scene in new[]{"Cooking","Dough","PizzaPrep","ToppingSort","PizzaDecorate","PizzaBake"})
            {
                SceneManager.LoadScene(scene);yield return null;yield return new WaitForSeconds(1.8f);
                var back=GameObject.Find("BackButton");Assert.IsNotNull(back.GetComponent<SceneBackButton>(),scene);
                yield return Tap(back.transform.position);
                Assert.IsTrue(ExitConfirmation.IsOpen,scene);Assert.AreEqual(0,Time.timeScale);
                float paused=Time.time;yield return new WaitForSecondsRealtime(.1f);Assert.AreEqual(paused,Time.time,.001f);
                Assert.AreEqual(PointerPhase.None,PointerInput.Read(Camera.main).phase);
                if(scene=="Cooking")Capture("cooking_confirmation");
                var modal=Object.FindFirstObjectByType<ExitConfirmation>();yield return Tap(modal.CancelPosition);
                Assert.IsFalse(ExitConfirmation.IsOpen);Assert.AreEqual(1,Time.timeScale);Assert.AreEqual(scene,SceneManager.GetActiveScene().name);
                yield return Tap(back.transform.position);modal=Object.FindFirstObjectByType<ExitConfirmation>();yield return Tap(modal.ClosePosition);
                Assert.IsFalse(ExitConfirmation.IsOpen);Assert.AreEqual(scene,SceneManager.GetActiveScene().name);
                yield return Tap(back.transform.position);modal=Object.FindFirstObjectByType<ExitConfirmation>();yield return Tap(modal.ConfirmPosition);
                yield return new WaitForSeconds(1.8f);Assert.AreEqual("Kitchen",SceneManager.GetActiveScene().name);Assert.AreEqual(1,Time.timeScale);
            }
        }
        [UnityTest] public IEnumerator DialogFitsFourAspectRatiosAndOutsideTapDoesNothing()
        {
            SceneManager.LoadScene("Cooking");yield return null;yield return new WaitForSeconds(1.8f);
            foreach(float ratio in new[]{16f/9,4f/3,21f/9,2.2f})
            {
                Camera.main.aspect=ratio;Camera.main.GetComponent<CameraFramingFitter>().Refit();
                foreach(var anchor in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None))anchor.Refresh();
                yield return null;
                yield return Tap(GameObject.Find("BackButton").transform.position);
                var modal=Object.FindFirstObjectByType<ExitConfirmation>();Assert.IsNotNull(modal);
                foreach(var sr in modal.GetComponentsInChildren<SpriteRenderer>())
                {
                    if(sr.name=="Dim")continue;
                    Vector3 a=Camera.main.WorldToViewportPoint(sr.bounds.min),b=Camera.main.WorldToViewportPoint(sr.bounds.max);
                    Assert.That(a.x,Is.InRange(0f,1f));Assert.That(a.y,Is.InRange(0f,1f));Assert.That(b.x,Is.InRange(0f,1f));Assert.That(b.y,Is.InRange(0f,1f));
                }
                yield return Tap(Camera.main.ViewportToWorldPoint(new Vector3(.05f,.5f,10)));
                Assert.IsTrue(ExitConfirmation.IsOpen);yield return Tap(modal.CancelPosition);Assert.IsFalse(ExitConfirmation.IsOpen);
            }
            Camera.main.ResetAspect();
        }
    }
}
