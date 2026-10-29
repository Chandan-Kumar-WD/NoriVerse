using System;
using System.Collections.Generic;
using NoriVerse.Core;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>
    /// Builds the splash scene, its intro AnimationClip and Animator from the timing measured in the reference video
    /// (see Docs/Splash_Video_001_Handoff.md). Run from the menu, or in batch mode with
    /// -executeMethod NoriVerse.EditorTools.SplashSceneBuilder.Build
    /// All positions are Unity units; the camera is orthographic, size 5 (10 units tall), centred on the origin.
    /// </summary>
    public static class SplashSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Splash.unity";
        public const string ClipPath = "Assets/_Project/Animations/Splash/SplashIntro.anim";
        private const string ControllerPath = "Assets/_Project/Animations/Splash/SplashIntro.controller";
        private const string Art = ArtSpec.ArtRoot;

        // --- Layout (Unity units) -------------------------------------------------------------------------
        private const float ScreenBottom = -5f;
        private const float TitleYIntro = 1.2f, TaglineYIntro = -0.4f;
        private const float TitleScaleIntro = 0.955f;
        private const float TitleYFinal = 2.92f, TaglineYFinal = 1.78f;
        private const float TitleScaleFinal = 0.70f;
        private const float TaglineWidthRatio = 1.0f;     // tagline spans ~ the same width as the title
        private const float StageFeetY = -4.53f;           // full-body Nori's soles
        private const float TableTopY = -2.78f;
        private const float TableHeightScale = 0.55f;
        private const float ClocheTableX = 0f;

        // --- Timeline (seconds, from the reference video) ---------------------------------------------------
        private const float PawsRiseStart = 0.07f, PawsRiseEnd = 0.27f, HandsGone = 0.52f;
        private const float HeadRiseStart = 0.32f, HeadRiseEnd = 0.55f;
        private const float LettersStart = 0.80f, LetterGap = 0.105f, LetterPop = 0.22f;
        private const float TaglineFadeStart = 1.95f, TaglineFadeEnd = 2.25f;
        private const float PeekSinkStart = 2.18f, PeekSinkEnd = 2.38f;
        private const float LogoMoveStart = 2.38f, LogoMoveEnd = 2.62f;
        private const float JumpStart = 2.39f, JumpApex = 2.67f, Landing = 2.84f, Settled = 2.95f;
        private const float TableSlideStart = 2.80f, TableSlideEnd = 3.47f;
        private const float ClocheLiftStart = 4.70f, ClocheGone = 5.00f;
        private const float FadeStart = 11.07f, FadeEnd = 11.40f, ClipEnd = 11.45f;

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigurePlayer();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = CreateCamera();
            var root = new GameObject("SplashRoot");
            var animator = root.AddComponent<Animator>();
            var splashController = root.AddComponent<SplashController>();
            var controllerObject = new SerializedObject(splashController);
            controllerObject.FindProperty("nextSceneName").stringValue = "Kitchen";   // the pot from the splash's end opens it
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            // Logo ----------------------------------------------------------------------------------------
            var logo = Child(root.transform, "Logo");
            var letters = new List<Transform>();
            var letterSprites = ArtSpec.LogoLetters();
            for (int i = 0; i < letterSprites.Count; i++)
            {
                var (spriteFile, x) = letterSprites[i];
                var sr = Sprite(logo, "L" + i, Art + "UI/Logo/" + spriteFile, new Vector3(x, 0f, 0f), 40);
                letters.Add(sr.transform);
            }
            float titleWidth = (letterSprites[letterSprites.Count - 1].x - letterSprites[0].x) + 1.1f;
            var tagline = Sprite(root.transform, "Tagline", Art + "UI/Logo/tagline.png", Vector3.zero, 40);
            float taglineNativeWidth = tagline.sprite.bounds.size.x;
            float taglineScaleIntro = titleWidth * TitleScaleIntro * TaglineWidthRatio / taglineNativeWidth;

            // Peek (Nori's head and paws over the bottom edge) --------------------------------------------
            var peekHead = Sprite(root.transform, "PeekHead", Art + "Characters/Nori/nori_peek_smile.png", new Vector3(0, ScreenBottom, 0), PeekHeadOrder);
            var peekHands = CreatePeekHands(root.transform);

            // Full-body Nori ------------------------------------------------------------------------------
            var shadowStage = Sprite(root.transform, "StageShadow", Art + "FX/ground_shadow.png", new Vector3(0, StageFeetY + 0.08f, 0), 0);
            shadowStage.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
            var stage = Sprite(root.transform, "NoriStage", Art + "Characters/Nori/nori_stand.png", new Vector3(0, StageFeetY, 0), StageOrder);


            // Table, dish cover, noodle bowl ----------------------------------------------------------------
            var tableGroup = Child(root.transform, "TableGroup");
            var tableShadow = Sprite(tableGroup, "TableShadow", Art + "FX/ground_shadow.png", new Vector3(0, -4.08f, 0), 14);
            tableShadow.transform.localScale = new Vector3(1.25f, 0.8f, 1f);
            var table = Sprite(tableGroup, "Table", Art + "Props/table.png", Vector3.zero, 20);
            float tableH = table.sprite.bounds.size.y * TableHeightScale;
            table.transform.localScale = new Vector3(1f, TableHeightScale, 1f);
            table.transform.localPosition = new Vector3(0, TableTopY - tableH / 2f, 0);
            var bowl = Sprite(tableGroup, "NoodleBowl", Art + "Props/noodle_bowl.png", new Vector3(0.02f, TableTopY - 0.10f, 0), 22);
            var cloche = Sprite(tableGroup, "Cloche", Art + "Props/cloche.png", new Vector3(ClocheTableX, TableTopY - 0.06f, 0), 24);

            // Hearts --------------------------------------------------------------------------------------
            var heartPositions = new[]
            {
                (new Vector2(-2.15f, -0.25f), 0.42f), (new Vector2(-1.30f, 0.65f), 0.30f), (new Vector2(0.05f, 0.98f), 0.52f),
                (new Vector2(1.35f, 0.65f), 0.30f), (new Vector2(2.15f, -0.20f), 0.44f)
            };
            var hearts = new List<Transform>();
            float heartNative = AssetDatabase.LoadAssetAtPath<Sprite>(Art + "FX/heart.png").bounds.size.x;
            for (int i = 0; i < heartPositions.Length; i++)
            {
                var h = Sprite(root.transform, "Heart" + i, Art + "FX/heart.png", heartPositions[i].Item1, 30);
                h.transform.localScale = Vector3.one * (heartPositions[i].Item2 / heartNative);
                hearts.Add(h.transform);
            }

            // Fade to white --------------------------------------------------------------------------------
            var fade = Sprite(root.transform, "FadeOverlay", Art + "FX/white_pixel.png", new Vector3(0, 0, -1f), 100);
            fade.color = new Color32(253, 253, 253, 0);
            fade.transform.localScale = new Vector3(40f / fade.sprite.bounds.size.x, 24f / fade.sprite.bounds.size.y, 1f);
            fade.gameObject.AddComponent<ScreenCoverFitter>();

            // Animation -----------------------------------------------------------------------------------
            var clip = BuildClip(root.transform, logo, letters, tagline, taglineScaleIntro, peekHead, peekHands, shadowStage, stage,
                tableGroup, tableShadow, bowl, cloche, hearts, fade);
            var controller = BuildController(clip);
            animator.runtimeAnimatorController = controller;
            stage.gameObject.AddComponent<NoriVerse.Gameplay.Splash.SplashPoseSmoother>().Configure(AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Shaders/SplashPoseBlend.shader"));

            // Save the scene in its first-frame state (blank white) so it looks right in the editor before Play;
            // the Animator drives everything from here once the game runs.
            clip.SampleAnimation(root, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuilderUtil.RegisterScenes(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("NoriVerse: splash scene built at " + ScenePath);
        }

        [MenuItem("NoriVerse/Build Splash Scene")]
        private static void BuildFromMenu() => Build();

        // ================================================================================================
        private static void ConfigurePlayer()
        {
            PlayerSettings.productName = "NoriVerse";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0, 0, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(253, 253, 253, 255);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 50f;
            go.AddComponent<CameraFramingFitter>();
            return cam;
        }

        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static SpriteRenderer Sprite(Transform parent, string name, string assetPath, Vector3 localPos, int order)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) throw new InvalidOperationException("Missing sprite: " + assetPath);
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        private const int PeekHeadOrder = 5, StageOrder = 10;

        private const float HandVisible = 0.85f;   // how far the raised hands stick up above the screen edge

        /// <summary>Two raised forearm-and-paw sprites (one mirrored) that reach up from below the edge before the head appears.</summary>
        private static List<SpriteRenderer> CreatePeekHands(Transform parent)
        {
            var result = new List<SpriteRenderer>();
            float handHeight = 600f / ArtSpec.HandPpu;
            foreach (float x in new[] { -2.35f, 2.25f })
            {
                var sr = Sprite(parent, x < 0 ? "PeekHandL" : "PeekHandR", Art + "Characters/Nori/nori_hand_up.png",
                    new Vector3(x, ScreenBottom + HandVisible - handHeight, 0), 6);
                sr.flipX = x > 0;
                result.Add(sr);
            }
            return result;
        }

        private static AnimatorController BuildController(AnimationClip clip)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var state = controller.layers[0].stateMachine.AddState("Intro");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
            return controller;
        }

        // ================================================================================================
        // Timeline
        // ================================================================================================
        private static AnimationClip BuildClip(Transform root, Transform logo, List<Transform> letters, SpriteRenderer tagline,
            float taglineScaleIntro, SpriteRenderer peekHead, List<SpriteRenderer> peekHands, SpriteRenderer shadowStage, SpriteRenderer stage,
            Transform tableGroup, SpriteRenderer tableShadow, SpriteRenderer bowl, SpriteRenderer cloche, List<Transform> hearts,
            SpriteRenderer fade)
        {
            var b = new ClipBuilder(root);

            // ---- Title: letters pop in, tagline fades in, then both shrink up (phase A -> B) -------------
            b.Pos(logo, 0f, 0f, TitleYIntro);
            b.Pos(logo, LogoMoveStart, 0f, TitleYIntro);
            b.Ease(logo, "localPosition.y", LogoMoveStart, LogoMoveEnd, TitleYIntro, TitleYFinal, Ease.InOutCubic);
            b.Scale(logo, 0f, TitleScaleIntro, TitleScaleIntro);
            b.Scale(logo, LogoMoveStart, TitleScaleIntro, TitleScaleIntro);
            b.EaseScale(logo, LogoMoveStart, LogoMoveEnd, TitleScaleIntro, TitleScaleFinal, Ease.InOutCubic);

            for (int i = 0; i < letters.Count; i++)
            {
                float t0 = LettersStart + i * LetterGap;
                b.Scale(letters[i], 0f, 0f, 0f);
                b.Scale(letters[i], t0, 0f, 0f);
                // overshoot pop: 0 -> 1.18 -> 1.0
                b.Ease(letters[i], "localScale.x", t0, t0 + LetterPop * 0.6f, 0f, 1.18f, Ease.OutCubic);
                b.Ease(letters[i], "localScale.y", t0, t0 + LetterPop * 0.6f, 0f, 1.18f, Ease.OutCubic);
                b.Ease(letters[i], "localScale.x", t0 + LetterPop * 0.6f, t0 + LetterPop, 1.18f, 1f, Ease.InOutCubic);
                b.Ease(letters[i], "localScale.y", t0 + LetterPop * 0.6f, t0 + LetterPop, 1.18f, 1f, Ease.InOutCubic);
            }

            var tl = tagline.transform;
            b.Pos(tl, 0f, 0f, TaglineYIntro);
            b.Pos(tl, LogoMoveStart, 0f, TaglineYIntro);
            b.Ease(tl, "localPosition.y", LogoMoveStart, LogoMoveEnd, TaglineYIntro, TaglineYFinal, Ease.InOutCubic);
            b.Scale(tl, 0f, taglineScaleIntro, taglineScaleIntro);
            b.Scale(tl, LogoMoveStart, taglineScaleIntro, taglineScaleIntro);
            b.EaseScale(tl, LogoMoveStart, LogoMoveEnd, taglineScaleIntro, taglineScaleIntro * TitleScaleFinal / TitleScaleIntro, Ease.InOutCubic);
            b.Alpha(tagline, 0f, 0f);
            b.Alpha(tagline, TaglineFadeStart, 0f);
            b.Alpha(tagline, TaglineFadeEnd, 1f);

            // ---- Peek: paws rise first, then the head ---------------------------------------------------------
            const float below = -4.8f;
            foreach (var hand in peekHands)
            {
                var ht = hand.transform;
                float restY = ht.localPosition.y;
                float hiddenY = restY - 1.4f;
                b.Active(ht, 0f, false);
                b.Active(ht, PawsRiseStart, true);
                b.Active(ht, HandsGone, false);
                b.Pos(ht, 0f, ht.localPosition.x, hiddenY);
                b.Pos(ht, PawsRiseStart, ht.localPosition.x, hiddenY);
                b.Ease(ht, "localPosition.y", PawsRiseStart, PawsRiseEnd, hiddenY, restY, Ease.OutCubic);
                b.Ease(ht, "localPosition.y", HeadRiseStart + 0.02f, HandsGone, restY, hiddenY, Ease.InOutCubic);
            }

            b.Active(peekHead.transform, 0f, false);
            b.Active(peekHead.transform, HeadRiseStart, true);
            b.Active(peekHead.transform, PeekSinkEnd, false);
            b.Pos(peekHead.transform, 0f, 0f, ScreenBottom + below);
            b.Pos(peekHead.transform, HeadRiseStart, 0f, ScreenBottom + below);
            b.Ease(peekHead.transform, "localPosition.y", HeadRiseStart, HeadRiseEnd - 0.08f, ScreenBottom + below, ScreenBottom + 0.10f, Ease.InOutCubic);
            b.Ease(peekHead.transform, "localPosition.y", HeadRiseEnd - 0.08f, HeadRiseEnd, ScreenBottom + 0.10f, ScreenBottom, Ease.InOutCubic);
            b.Ease(peekHead.transform, "localPosition.y", PeekSinkStart, PeekSinkEnd, ScreenBottom, ScreenBottom + below, Ease.InOutCubic);

            var smile = Load("Characters/Nori/nori_peek_smile.png");
            var talk = Load("Characters/Nori/nori_peek_talk.png");
            var ooh = Load("Characters/Nori/nori_peek_ooh.png");
            var blink = Load("Characters/Nori/nori_peek_blink.png");
            b.Sprite(peekHead, 0f, smile);
            b.Sprite(peekHead, 0.60f, blink);
            b.Sprite(peekHead, 0.70f, smile);
            for (int i = 0; i < letters.Count; i++) // mouth flaps in time with each letter
            {
                float t0 = LettersStart + i * LetterGap;
                b.Sprite(peekHead, t0, talk);
                b.Sprite(peekHead, t0 + 0.065f, smile);
            }
            b.Sprite(peekHead, 1.78f, ooh);
            b.Sprite(peekHead, 2.12f, smile);

            // ---- Full-body Nori: jump in, land, then the cooking sequence ----------------------------------------
            // Once the table has arrived the figure below its hem is cut away, so the _cut sprites are used from then on.
            Sprite P(string pose, bool cut = false) => Load("Characters/Nori/nori_" + pose + (cut ? "_cut" : "") + ".png");

            var st = stage.transform;
            b.Active(st, 0f, false);
            b.Active(st, JumpStart, true);
            b.Pos(st, 0f, 0f, StageFeetY - 6.2f);
            b.Pos(st, JumpStart, 0f, StageFeetY - 6.2f);
            b.Ease(st, "localPosition.y", JumpStart, JumpApex, StageFeetY - 6.2f, StageFeetY + 0.6f, Ease.RiseBlend);
            b.Ease(st, "localPosition.y", JumpApex, Landing, StageFeetY + 0.6f, StageFeetY, Ease.InCubic);
            // squash on landing (feet pivot keeps the soles planted)
            b.Scale(st, 0f, 1f, 1f);
            b.Scale(st, JumpStart, 1f, 1f);
            b.Ease(st, "localScale.x", JumpStart, JumpApex, 0.94f, 0.97f, Ease.OutCubic);   // stretch on the way up
            b.Ease(st, "localScale.y", JumpStart, JumpApex, 1.05f, 1.03f, Ease.OutCubic);
            b.Ease(st, "localScale.x", JumpApex, Landing, 0.97f, 1f, Ease.InCubic);
            b.Ease(st, "localScale.y", JumpApex, Landing, 1.03f, 1f, Ease.InCubic);
            b.Ease(st, "localScale.x", Landing, Landing + 0.07f, 1f, 1.07f, Ease.OutCubic);
            b.Ease(st, "localScale.y", Landing, Landing + 0.07f, 1f, 0.93f, Ease.OutCubic);
            b.Ease(st, "localScale.x", Landing + 0.07f, Settled, 1.07f, 1f, Ease.InOutCubic);
            b.Ease(st, "localScale.y", Landing + 0.07f, Settled, 0.93f, 1f, Ease.InOutCubic);

            // gentle breathing while idle (8.75s -> fade)
            for (float t = 8.75f; t < FadeStart; t += 0.8f)
            {
                b.Ease(st, "localScale.y", t, t + 0.4f, 1f, 1.012f, Ease.InOutCubic);
                b.Ease(st, "localScale.y", t + 0.4f, t + 0.8f, 1.012f, 1f, Ease.InOutCubic);
            }
            // happy bounce while the arms are spread (5.25 -> 6.05)
            for (int k = 0; k < 2; k++)
            {
                float t = 5.25f + k * 0.4f;
                b.Ease(st, "localPosition.y", t, t + 0.2f, StageFeetY, StageFeetY + 0.1f, Ease.OutCubic);
                b.Ease(st, "localPosition.y", t + 0.2f, t + 0.4f, StageFeetY + 0.1f, StageFeetY, Ease.InCubic);
            }
            // leaning in toward the dish: small rotation to the right
            b.Rot(st, 0f, 0f);
            b.Rot(st, 3.30f, 0f);
            b.Ease(st, "localEulerAnglesRaw.z", 3.30f, 3.70f, 0f, -2.5f, Ease.InOutCubic);
            b.Ease(st, "localEulerAnglesRaw.z", 4.60f, 4.90f, -2.5f, 0f, Ease.InOutCubic);

            const float cutAt = 3.42f;
            b.Sprite(stage,0f,P("stand"));
            b.Sprite(stage,Settled,P("stand_blink"));
            b.Sprite(stage,3.07f,P("stand"));
            b.Sprite(stage,3.33f,P("lean_curious"));
            b.Sprite(stage,cutAt,P("lean_curious",true));
            b.Sprite(stage,4.85f,P("surprised",true));
            b.Sprite(stage,5.10f,P("stand_blink",true));
            b.Sprite(stage,5.25f,P("arms_spread",true));
            // Keep the established artwork; allow each continuous eating beat to read.
            b.Sprite(stage,6.05f,P("hold_chopsticks",true));
            b.Sprite(stage,6.55f,P("eat",true));
            b.Sprite(stage,7.05f,P("hold_chopsticks",true));
            b.Sprite(stage,7.50f,P("eat",true));
            b.Sprite(stage,8.00f,P("chew",true));
            b.Sprite(stage,8.65f,P("hold_chopsticks",true));
            b.Sprite(stage,9.10f,P("hold_blink",true));
            b.Sprite(stage,9.26f,P("hold_chopsticks",true));
            b.Sprite(stage,10.50f,P("hold_blink",true));
            b.Sprite(stage,10.66f,P("hold_chopsticks",true));

            foreach(float biteTime in new[]{6.45f,7.40f})
            {
                b.Ease(st,"localPosition.y",biteTime,biteTime+.26f,StageFeetY,StageFeetY+.045f,Ease.InOutCubic);
                b.Ease(st,"localPosition.y",biteTime+.26f,biteTime+.54f,StageFeetY+.045f,StageFeetY,Ease.InOutCubic);
            }

            // stage shadow: appears when he lands, goes away as the table arrives
            b.Active(shadowStage.transform, 0f, false);
            b.Active(shadowStage.transform, Landing - 0.1f, true);
            b.Alpha(shadowStage, 0f, 0f);
            b.Alpha(shadowStage, Landing - 0.1f, 0f);
            b.Alpha(shadowStage, Landing + 0.1f, 1f);
            b.Alpha(shadowStage, 3.30f, 1f);
            b.Alpha(shadowStage, 3.55f, 0f);

            // ---- Table with the covered dish slides in from the left ------------------------------------------
            var tg = tableGroup;
            b.Active(tg, 0f, false);
            b.Active(tg, TableSlideStart, true);
            b.Pos(tg, 0f, -7.4f, 0f);
            b.Pos(tg, TableSlideStart, -7.4f, 0f);
            b.Ease(tg, "localPosition.x", TableSlideStart, TableSlideEnd, -7.4f, 0f, Ease.EaseOutSoft);
            foreach (var sr in new[] { tableShadow, cloche })
            {
                b.Alpha(sr, 0f, 0f);
                b.Alpha(sr, TableSlideStart + 0.05f, 0f);
                b.Alpha(sr, TableSlideStart + 0.28f, 1f);
            }
            var tableSr = tg.Find("Table").GetComponent<SpriteRenderer>();
            b.Alpha(tableSr, 0f, 0f);
            b.Alpha(tableSr, TableSlideStart + 0.05f, 0f);
            b.Alpha(tableSr, TableSlideStart + 0.28f, 1f);

            // bowl is hidden under the cover until it is lifted
            b.Alpha(bowl, 0f, 0f);
            b.Alpha(bowl, ClocheLiftStart, 0f);
            b.Alpha(bowl, ClocheLiftStart + 0.10f, 1f);

            // cover flies up and to the right while fading out
            var cl = cloche.transform;
            Vector3 clocheRest = cl.localPosition;
            b.Pos(cl, 0f, clocheRest.x, clocheRest.y);
            b.Pos(cl, ClocheLiftStart, clocheRest.x, clocheRest.y);
            b.Ease(cl, "localPosition.x", ClocheLiftStart, ClocheGone, clocheRest.x, clocheRest.x + 2.1f, Ease.OutCubic);
            b.Ease(cl, "localPosition.y", ClocheLiftStart, ClocheGone, clocheRest.y, clocheRest.y + 3.1f, Ease.OutCubic);
            b.Rot(cl, 0f, 0f);
            b.Rot(cl, ClocheLiftStart, 0f);
            b.Ease(cl, "localEulerAnglesRaw.z", ClocheLiftStart, ClocheGone, 0f, -38f, Ease.OutCubic);
            b.Alpha(cloche, ClocheLiftStart, 1f);
            b.Alpha(cloche, ClocheLiftStart + 0.05f, 0.95f);
            b.Alpha(cloche, ClocheGone, 0f);

            // ---- Hearts pop around his head while he enjoys the noodles ---------------------------------------------
            float[] heartStart = { 7.80f, 7.88f, 7.84f, 7.92f, 7.86f };
            for (int i = 0; i < hearts.Count; i++)
            {
                var h = hearts[i];
                var sr = h.GetComponent<SpriteRenderer>();
                float full = h.localScale.x;
                float t0 = heartStart[i];
                b.Scale(h, 0f, 0f, 0f);
                b.Scale(h, t0, 0f, 0f);
                b.Ease(h, "localScale.x", t0, t0 + 0.18f, 0f, full * 1.25f, Ease.OutCubic);
                b.Ease(h, "localScale.y", t0, t0 + 0.18f, 0f, full * 1.25f, Ease.OutCubic);
                b.Ease(h, "localScale.x", t0 + 0.18f, t0 + 0.32f, full * 1.25f, full, Ease.InOutCubic);
                b.Ease(h, "localScale.y", t0 + 0.18f, t0 + 0.32f, full * 1.25f, full, Ease.InOutCubic);
                b.Ease(h, "localScale.x", t0 + 0.32f, t0 + 0.55f, full, full * 1.12f, Ease.InOutCubic);
                b.Ease(h, "localScale.y", t0 + 0.32f, t0 + 0.55f, full, full * 1.12f, Ease.InOutCubic);
                b.Ease(h, "localScale.x", t0 + 0.55f, t0 + 0.80f, full * 1.12f, full, Ease.InOutCubic);
                b.Ease(h, "localScale.y", t0 + 0.55f, t0 + 0.80f, full * 1.12f, full, Ease.InOutCubic);
                b.Alpha(sr, 0f, 0f);
                b.Alpha(sr, t0, 0f);
                b.Alpha(sr, t0 + 0.12f, 1f);
                b.Alpha(sr, 8.35f, 1f);
                b.Alpha(sr, 8.80f, 0f);
                // hearts float slightly upward while fading
                Vector3 p = h.localPosition;
                b.Pos(h, 0f, p.x, p.y);
                b.Pos(h, t0, p.x, p.y);
                b.Ease(h, "localPosition.y", t0, 8.80f, p.y, p.y + 0.18f, Ease.OutCubic);
            }

            // ---- Fade to white, then tell the game the splash is over -------------------------------------------------
            b.Alpha(fade, 0f, 0f);
            b.Alpha(fade, FadeStart, 0f);
            b.Alpha(fade, FadeEnd, 1f);
            b.Alpha(fade, ClipEnd, 1f);

            return b.Finish(ClipPath, FadeEnd, ClipEnd);
        }

        private static Sprite Load(string rel)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(Art + rel);
            if (s == null) throw new InvalidOperationException("Missing sprite: " + rel);
            return s;
        }

        // ================================================================================================
        private static class Ease
        {
            public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
            public static float InCubic(float t) => t * t * t;
            public static float RiseBlend(float t) => (t + (1f - (1f - t) * (1f - t))) * 0.5f;
            public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);
            public static float EaseOutSoft(float t) => 0.7f * t + 0.3f * (1f - (1f - t) * (1f - t));
            public static float InOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        /// <summary>Collects keyed values per property, then writes them into one AnimationClip.</summary>
        private sealed class ClipBuilder
        {
            private struct Key { public float t, v; public bool step; public int seq; }

            private readonly Transform root;
            private readonly Dictionary<string, (EditorCurveBinding binding, List<Key> keys)> curves = new Dictionary<string, (EditorCurveBinding, List<Key>)>();
            private readonly Dictionary<string, (EditorCurveBinding binding, List<(float t, Sprite s)> keys)> sprites =
                new Dictionary<string, (EditorCurveBinding, List<(float, Sprite)>)>();

            public ClipBuilder(Transform root) { this.root = root; }

            private string PathOf(Transform t) => t == root ? "" : AnimationUtility.CalculateTransformPath(t, root);

            private void Add(Transform t, Type type, string prop, float time, float value, bool step)
            {
                string path = PathOf(t);
                string id = path + "|" + type.Name + "|" + prop;
                if (!curves.TryGetValue(id, out var entry))
                {
                    entry = (EditorCurveBinding.FloatCurve(path, type, prop), new List<Key>());
                    curves[id] = entry;
                }
                entry.keys.Add(new Key { t = time, v = value, step = step, seq = entry.keys.Count });
            }

            public void Pos(Transform t, float time, float x, float y) { Add(t, typeof(Transform), "localPosition.x", time, x, false); Add(t, typeof(Transform), "localPosition.y", time, y, false); }
            public void Scale(Transform t, float time, float x, float y) { Add(t, typeof(Transform), "localScale.x", time, x, false); Add(t, typeof(Transform), "localScale.y", time, y, false); }
            public void Rot(Transform t, float time, float z) => Add(t, typeof(Transform), "localEulerAnglesRaw.z", time, z, false);
            public void Active(Transform t, float time, bool on) => Add(t, typeof(GameObject), "m_IsActive", time, on ? 1f : 0f, true);
            public void Alpha(SpriteRenderer sr, float time, float a) => Add(sr.transform, typeof(SpriteRenderer), "m_Color.a", time, a, false);

            public void Ease(Transform t, string prop, float t0, float t1, float v0, float v1, Func<float, float> ease)
            {
                int steps = Mathf.Max(2, Mathf.CeilToInt((t1 - t0) * 60f));
                for (int i = 0; i <= steps; i++)
                {
                    float u = (float)i / steps;
                    Add(t, typeof(Transform), prop, Mathf.Lerp(t0, t1, u), Mathf.Lerp(v0, v1, ease(u)), false);
                }
            }

            public void EaseScale(Transform t, float t0, float t1, float from, float to, Func<float, float> ease)
            {
                Ease(t, "localScale.x", t0, t1, from, to, ease);
                Ease(t, "localScale.y", t0, t1, from, to, ease);
            }

            public void Sprite(SpriteRenderer sr, float time, Sprite sprite)
            {
                string path = PathOf(sr.transform);
                string id = path + "|sprite";
                if (!sprites.TryGetValue(id, out var entry))
                {
                    entry = (EditorCurveBinding.PPtrCurve(path, typeof(SpriteRenderer), "m_Sprite"), new List<(float, Sprite)>());
                    sprites[id] = entry;
                }
                entry.keys.Add((time, sprite));
            }

            public AnimationClip Finish(string clipPath, float eventTime, float length)
            {
                var clip = new AnimationClip { frameRate = 60f, name = "SplashIntro" };
                foreach (var (binding, keys) in curves.Values)
                {
                    keys.Sort((a, b) => a.t != b.t ? a.t.CompareTo(b.t) : a.seq.CompareTo(b.seq));
                    var curve = new AnimationCurve();
                    float lastT = float.NegativeInfinity;
                    var stepIndices = new List<int>();
                    foreach (var k in keys)
                    {
                        if (k.t - lastT < 1e-4f && curve.length > 0) { curve.RemoveKey(curve.length - 1); }
                        int idx = curve.AddKey(new Keyframe(k.t, k.v));
                        lastT = k.t;
                        if (k.step) stepIndices.Add(idx);
                    }
                    for (int i = 0; i < curve.length; i++)
                    {
                        AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(curve, i, stepIndices.Contains(i) ? AnimationUtility.TangentMode.Constant : AnimationUtility.TangentMode.Linear);
                    }
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }
                foreach (var (binding, keys) in sprites.Values)
                {
                    keys.Sort((a, b) => a.t.CompareTo(b.t));
                    var frames = new ObjectReferenceKeyframe[keys.Count];
                    for (int i = 0; i < keys.Count; i++) frames[i] = new ObjectReferenceKeyframe { time = keys[i].t, value = keys[i].s };
                    AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                }
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                AnimationUtility.SetAnimationEvents(clip, new[] { new AnimationEvent { time = eventTime, functionName = "OnIntroFinished" } });

                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(clipPath));
                AssetDatabase.DeleteAsset(clipPath);
                AssetDatabase.CreateAsset(clip, clipPath);
                return clip;
            }
        }
    }
}
