using System.Collections;
using NoriVerse.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    /// <summary>Runs the real splash scene in play mode and checks the timeline plays through to the end.</summary>
    public class SplashPlayModeTests
    {
        [SetUp] public void ResetProfile()=>PizzaTestProfile.Reset();

        private const string SceneName = "Splash";

        private static SpriteRenderer Find(string name)
        {
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (sr.name == name) return sr;
            return null;
        }

        [UnityTest]
        public IEnumerator SplashPlaysThroughAndCompletes()
        {
            SceneManager.LoadScene(SceneName);
            yield return null;
            yield return null;

            var controller = Object.FindFirstObjectByType<SplashController>();
            Assert.IsNotNull(controller, "SplashController missing from the Splash scene");
            Assert.IsNotNull(controller.GetComponent<Animator>().runtimeAnimatorController, "Animator has no controller");

            bool completed = false;
            controller.OnSplashComplete.AddListener(() => completed = true);

            var head = Find("PeekHead");
            var stage = Find("NoriStage");
            var table = Find("Table");
            var fade = Find("FadeOverlay");
            Assert.IsNotNull(head); Assert.IsNotNull(stage); Assert.IsNotNull(table); Assert.IsNotNull(fade);

            Assert.IsFalse(stage.gameObject.activeInHierarchy, "Full-body Nori must not show at the start");

            yield return new WaitForSeconds(1.0f);
            Assert.IsTrue(head.gameObject.activeInHierarchy, "Peek head should be visible at 1s");

            yield return new WaitForSeconds(2.5f);   // ~3.5s
            Assert.IsTrue(stage.gameObject.activeInHierarchy, "Full-body Nori should be on screen at 3.5s");
            Assert.IsFalse(head.gameObject.activeInHierarchy, "Peek head should be gone at 3.5s");
            Assert.IsTrue(table.gameObject.activeInHierarchy, "Table should have arrived at 3.5s");

            yield return new WaitForSeconds(1.9f);   // ~5.5s, safely inside the arms-spread window (5.25-6.10s)
            StringAssert.StartsWith("nori_arms_spread", stage.sprite.name, "Arms should be spread at ~5.5s");

            yield return new WaitForSeconds(7.0f);   // ~12.5s
            Assert.IsTrue(completed, "OnSplashComplete should have fired");
            Assert.IsTrue(controller.IsFinished);
            yield return null;
            Assert.AreEqual("Kitchen", SceneManager.GetActiveScene().name, "The splash should hand over to the kitchen scene");
        }
    }
}
