using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NoriVerse.Tests
{
    /// <summary>Renders the real running splash and saves screenshots so we can see what the player sees.</summary>
    public class SplashRenderCaptureTests
    {
        [SetUp] public void Setup(){PizzaTestProfile.Reset();Time.captureFramerate=60;}
        [TearDown] public void Teardown(){Time.captureFramerate=0;}
        /// <summary>Counts pixels in a region that differ from the near-white background.</summary>
        private static int NonBackgroundPixels(Texture2D tex, int x, int y, int w, int h)
        {
            int count = 0;
            var px = tex.GetPixels32();
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                {
                    var c = px[j * tex.width + i];
                    if (c.r < 235 || c.g < 235 || c.b < 235) count++;
                }
            return count;
        }

        [UnityTest]
        public IEnumerator CaptureRealRenderAtKeyTimes()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "playmode_captures"));
            dir=Path.Combine(dir,"splash_smooth_original");Directory.CreateDirectory(dir);
            SceneManager.LoadScene("Splash");
            yield return null;
            float start = Time.time;
            foreach (float t in new[] { 0.9f, 1.8f, 3.0f, 3.6f, 5.5f, 6.55f, 6.68f, 6.85f, 7.0f, 7.65f, 8.2f, 9.0f })
            {
                while (Time.time - start < t) yield return null;
                Assert.IsNull(GameObject.Find("DiningRig"),"Keep the original splash, not the discarded articulated version");
                if(Mathf.Approximately(t,6.68f))Assert.IsTrue(GameObject.Find("NoriStage").GetComponent<NoriVerse.Gameplay.Splash.SplashPoseSmoother>().IsBlending,"The original eating pose should enter through a smooth transition");
                // batch mode has no end-of-frame; render the live camera into a texture instead
                var cam = Camera.main;
                var rt = new RenderTexture(1920, 874, 24);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(1920, 874, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1920, 874), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                cam.targetTexture = null;
                if(System.Environment.GetEnvironmentVariable("NORIVERSE_CAPTURE_FRAMES")=="1")File.WriteAllBytes(Path.Combine(dir, $"play_{t:0.0}.png"), tex.EncodeToPNG());
                if (t >= 3.0f) Assert.Greater(NonBackgroundPixels(tex, 760, 300, 400, 330), 20000, $"Nori should be drawn at {t}s");
                Object.Destroy(tex);
                Object.Destroy(rt);
            }
            Assert.Pass();
        }
    }
}
