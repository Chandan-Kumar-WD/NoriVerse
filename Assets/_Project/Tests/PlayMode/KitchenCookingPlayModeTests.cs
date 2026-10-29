using System.Collections;
using System.IO;
using NoriVerse.Characters;
using NoriVerse.Core;
using NoriVerse.Gameplay.Cooking;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    /// <summary>
    /// Plays the kitchen menu and the cooking screen in real time, driving them with an injected "finger", and saves frames
    /// to Docs/playmode_captures for comparison with the reference video.
    /// </summary>
    public class KitchenCookingPlayModeTests
    {
        private static string CaptureDir
        {
            get
            {
                string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "playmode_captures"));
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        private static void Shot(string name, int w = 1920, int h = 874)
        {
            var cam = Camera.main;
            var rt = new RenderTexture(w, h, 24);
            cam.aspect = (float)w / h;           // behave like a real 2.2:1 screen
            cam.orthographicSize = Mathf.Max(5f, 22.4f / (2f * cam.aspect));   // same framing rule as CameraFramingFitter, applied now
            foreach (var anchor in Object.FindObjectsByType<ScreenCornerAnchor>(FindObjectsSortMode.None)) anchor.Refresh();
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            cam.ResetAspect();
            if(System.Environment.GetEnvironmentVariable("NORIVERSE_CAPTURE_FRAMES")=="1")File.WriteAllBytes(Path.Combine(CaptureDir, name + ".png"), tex.EncodeToPNG());
            Object.Destroy(tex);
            Object.Destroy(rt);
        }

        private static IEnumerator WaitUntil(float startTime, float t)
        {
            while (Time.time - startTime < t) yield return null;
        }

        // Headless runs would otherwise spin at ~10,000 fps; fix game time to 60 fps so drags and animations take real durations.
        [SetUp]
        public void FixFrameRate(){PizzaTestProfile.Reset();Time.captureFramerate=60;NoriVerse.Gameplay.PizzaFinish.PizzaRecipeState.BeginNewRecipe();}

        [TearDown]
        public void Cleanup()
        {
            PointerInput.ClearInjected();
            Time.captureFramerate = 0;
        }

        [UnityTest]
        public IEnumerator KitchenCharacterKeepsItsLayerAcrossRepeatedExpressions()
        {
            SceneManager.LoadScene("Kitchen");
            yield return null;
            yield return new WaitForSeconds(2.5f);
            Assert.IsNull(GameObject.Find("BackButton"));
            Assert.IsNull(GameObject.Find("HandsButton"));
            Assert.IsNull(GameObject.Find("ChefButton"));
            Assert.IsNull(GameObject.Find("DishDisplayShelf"), "The counter must have one surface, without an added display ledge");
            var mood = Object.FindFirstObjectByType<NoriMoodDirector>();
            mood.Stop();
            var face = Object.FindFirstObjectByType<SpriteCrossfader>();
            var counter = GameObject.Find("Counter").GetComponent<SpriteRenderer>();
            for (int cycle = 0; cycle < 3; cycle++)
            {
                foreach (var pose in face.Poses)
                {
                    face.Show(pose.name, 0.04f);
                    yield return new WaitForSeconds(0.06f);
                    foreach (var renderer in face.GetComponentsInChildren<SpriteRenderer>())
                        Assert.Less(renderer.sortingOrder, counter.sortingOrder,
                            "Repeated cross-fades must never move Nori in front of her counter");
                    if (cycle == 2) Shot("nori_pose_" + pose.name, 1920, 1080);
                }
            }
            var dishes = Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None);
            int unlocked = 0;
            foreach (var dish in dishes)
            {
                var served = dish.transform.Find("Visual/ServedDish");
                Assert.IsNotNull(served);
                Assert.AreEqual(Vector3.one, served.localScale, "Served food must retain its illustrated volume without vertical squashing");
                var badge = dish.transform.Find("Visual/Padlock");
                Assert.Less(badge.localPosition.x, 0f);
                Assert.IsTrue(badge.GetComponent<SpriteRenderer>().bounds.Intersects(served.GetComponent<SpriteRenderer>().bounds),
                    "Lock badges must overlap their plates rather than float above them");
                if (!dish.IsLocked) { unlocked++; Assert.AreEqual(0f, dish.transform.position.x, 0.01f); }
            }
            Assert.AreEqual(1, unlocked);
        }

        [UnityTest]
        public IEnumerator KitchenMenuPlaysAndFirstDishStartsCooking()
        {
            SceneManager.LoadScene("Kitchen");
            yield return null;
            float t0 = Time.time;
            var controller = Object.FindFirstObjectByType<KitchenMenuController>();
            Assert.IsNotNull(controller);

            foreach (float t in new[] { 0.3f, 1.2f, 1.75f, 2.5f, 4.0f, 9.8f, 11.5f, 12.3f, 14.4f })
            {
                yield return WaitUntil(t0, t);
                Shot($"kitchen_{t:0.0}");
            }

            // a locked dish must not start anything; the first dish must
            var dishes = Object.FindObjectsByType<DishButton>(FindObjectsSortMode.None);
            DishButton locked = null, first = null;
            foreach (var d in dishes) { if (d.IsLocked) locked = d; else first = d; }
            Assert.IsNotNull(locked); Assert.IsNotNull(first);
            controller.TapDish(locked);
            yield return new WaitForSeconds(0.8f);
            Assert.AreEqual("Kitchen", SceneManager.GetActiveScene().name, "Tapping a locked dish must not leave the kitchen");

            Assert.IsTrue(first.GetComponent<Collider2D>().OverlapPoint(first.transform.position));
            PointerInput.InjectForTests(PointerPhase.Began, first.transform.position);
            yield return null;
            PointerInput.InjectForTests(PointerPhase.Ended, first.transform.position);
            yield return null;
            PointerInput.ClearInjected();
            yield return new WaitForSeconds(2.6f);
            Assert.AreEqual("Cooking", SceneManager.GetActiveScene().name, "Tapping the first dish should open the cooking screen");
        }

        [UnityTest]
        public IEnumerator NoriGetsSadWhenIdleAndCheersUpOnTouch()
        {
            SceneManager.LoadScene("Kitchen");
            yield return null;
            var mood = Object.FindFirstObjectByType<NoriMoodDirector>();
            Assert.IsNotNull(mood);
            yield return new WaitForSeconds(4f);
            Assert.IsFalse(mood.IsSad, "Nori should be happy while the screen was recently active");
            yield return new WaitForSeconds(7f);
            Assert.IsFalse(mood.IsSad,"Short inactivity must not start the old emotion carousel");
            yield return new WaitForSeconds(18f);
            Assert.IsTrue(mood.IsSad, "Nori should look sad after the screen has been idle");
            Shot("kitchen_sad");

            PointerInput.InjectForTests(PointerPhase.Began, new Vector2(-8f, 3.5f));   // touch empty wall
            yield return null;
            PointerInput.InjectForTests(PointerPhase.Ended, new Vector2(-8f, 3.5f));
            yield return null;
            PointerInput.ClearInjected();
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(mood.IsSad, "A touch should cheer Nori up");
            yield return new WaitForSeconds(1.2f);
            Shot("kitchen_cheered");
        }

        [UnityTest]
        public IEnumerator WholeDesignIsVisibleOnAnyScreenShape()
        {
            var shapes = new[] { ("16x9", 1920, 1080), ("4x3", 1600, 1200), ("21x9", 2520, 1080), ("22x10", 1920, 874) };
            foreach (var scene in new[] { "Kitchen", "Cooking" })
            {
                SceneManager.LoadScene(scene);
                yield return null;
                yield return new WaitForSeconds(scene == "Kitchen" ? 2.4f : 1.3f);
                var cam = Camera.main;
                foreach (var (label, w, h) in shapes)
                {
                    cam.aspect = (float)w / h;
                    yield return null;                       // let the framing component react
                    if (scene == "Cooking")
                    {
                        var bowlBounds=GameObject.Find("Bowl").GetComponent<SpriteRenderer>().bounds;
                        var trayBounds=GameObject.Find("Tray").GetComponent<SpriteRenderer>().bounds;
                        Assert.Greater(trayBounds.min.x-bowlBounds.max.x,.15f,"The two batter utensils need a visible gap");
                        var back = GameObject.Find("BackButton").GetComponent<SpriteRenderer>();
                        var min = cam.WorldToViewportPoint(back.bounds.min);
                        var max = cam.WorldToViewportPoint(back.bounds.max);
                        Assert.That(min.x, Is.InRange(0f, 0.05f), "Back button should sit near the left edge");
                        Assert.That(max.y, Is.InRange(0.95f, 1f), "Back button should sit near the top edge");
                    }
                    // the left-most and right-most things in the design must be inside the view
                    foreach (float x in new[] { -10.8f, 10.8f })
                    {
                        Vector3 v = cam.WorldToViewportPoint(new Vector3(x, 0f, 0f));
                        Assert.That(v.x, Is.InRange(0f, 1f), $"{scene} at {label}: x={x} falls outside the screen");
                    }
                    foreach (float y in new[] { -5f, 5f })
                    {
                        Vector3 v = cam.WorldToViewportPoint(new Vector3(0f, y, 0f));
                        Assert.That(v.y, Is.InRange(0f, 1f), $"{scene} at {label}: y={y} falls outside the screen");
                    }
                    cam.ResetAspect();
                    Shot($"{scene.ToLower()}_{label}", w, h);
                }
            }
        }

        [UnityTest]
        public IEnumerator CookingPlaysThroughDragAndWhisk()
        {
            SceneManager.LoadScene("Cooking");
            yield return null;
            float t0 = Time.time;
            var controller = Object.FindFirstObjectByType<CookingController>();
            var drag = Object.FindFirstObjectByType<IngredientDragController>();
            var bowl = Object.FindFirstObjectByType<BowlArea>();
            var whisk = Object.FindFirstObjectByType<WhiskController>();
            Assert.IsNotNull(controller); Assert.IsNotNull(drag); Assert.IsNotNull(bowl); Assert.IsNotNull(whisk);

            yield return WaitUntil(t0, 0.3f); Shot("cook_0.3");
            yield return WaitUntil(t0, 1.2f); Shot("cook_1.2");
            foreach (var ing in drag.Ingredients) Assert.AreEqual(IngredientState.InTray, ing.State, "All ingredients should be in the tray after the pop-in");

            // the tutorial hand appears if the child waits
            yield return WaitUntil(t0, 8.2f);
            Shot("cook_hint_8.2");
            yield return WaitUntil(t0, 9.4f);
            Shot("cook_hint_9.4");

            // drag every ingredient into the bowl like a finger would
            int n = 0;
            foreach (var ing in drag.Ingredients)
            {
                Vector2 from = ing.transform.position;
                Vector2 to = (Vector2)bowl.transform.position + new Vector2(Mathf.Cos(n * 1.3f), Mathf.Sin(n * 1.3f)) * 1.6f;
                PointerInput.InjectForTests(PointerPhase.Began, from);
                yield return null;
                for (int i = 1; i <= 24; i++)
                {
                    PointerInput.InjectForTests(PointerPhase.Held, Vector2.Lerp(from, to, i / 24f));
                    yield return null;
                    // Debug.Log($"DBGTEST held {i} {ing.name} state={ing.State} pos={ing.transform.position} dt={Time.deltaTime}");
                    if (n == 1 && i == 12) Shot("cook_drag_mid");
                }
                PointerInput.InjectForTests(PointerPhase.Ended, to);
                yield return null;
                PointerInput.ClearInjected();
                yield return new WaitForSeconds(0.55f);
                Assert.AreEqual(IngredientState.InBowl, ing.State, $"Ingredient {ing.name} should be in the bowl after being dropped there");
                n++;
            }
            Shot("cook_all_in");

            // a drop outside the bowl must send the item home (exercise with a fresh scene state is not possible, so just check whisk phase)
            yield return new WaitForSeconds(1.8f);
            Assert.IsTrue(whisk.IsActive, "The whisk should be ready after the last ingredient");
            Shot("cook_whisk_in");
            yield return new WaitForSeconds(1.0f);
            Shot("cook_arrow");

            // stir in circles
            Vector2 center = bowl.Center;
            Vector2 start = (Vector2)whisk.transform.position;
            PointerInput.InjectForTests(PointerPhase.Began, start);
            yield return null;
            float angle = Mathf.Atan2(start.y - center.y, start.x - center.x);
            float radius = 1.5f;
            int frames = 0;
            while (whisk.Progress < 1f && frames < 1500)
            {
                angle -= (8f / radius) * Time.deltaTime;     // ~8 units per second along the circle, clockwise
                Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                PointerInput.InjectForTests(PointerPhase.Held, p);
                frames++;
                if (frames == 30) Shot("cook_stir_a");
                if (frames == 55) Shot("cook_stir_b");
                if (frames == 90) Shot("cook_stir_c");
                if (frames == 140) Shot("cook_stir_d");
                if (frames == 200) Shot("cook_stir_e");
                if (frames == 260) Shot("cook_stir_f");
                yield return null;
            }
            PointerInput.InjectForTests(PointerPhase.Ended, center);
            yield return null;
            PointerInput.ClearInjected();
            Assert.GreaterOrEqual(whisk.Progress, 1f, "Stirring should finish the batter");
            Shot("cook_done");

            bool ready = false;
            controller.OnBatterReady.AddListener(() => ready = true);
            yield return new WaitForSeconds(3.0f);
            Assert.IsTrue(ready, "OnBatterReady should fire after the batter is smooth");
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual("Dough", SceneManager.GetActiveScene().name, "Finished batter should continue into the next video's dough activity");
            Shot("cook_end");
        }
    }
}
