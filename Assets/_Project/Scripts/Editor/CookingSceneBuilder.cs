using NoriVerse.Gameplay.Cooking;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static NoriVerse.EditorTools.SceneBuilderUtil;

namespace NoriVerse.EditorTools
{
    /// <summary>
    /// Builds the "mix the batter" scene (Video 002, 18-58 s). Layout from Docs/Kitchen_Video_002_Breakdown.md.
    /// Run via NoriVerse/Build Cooking Scene or -executeMethod NoriVerse.EditorTools.CookingSceneBuilder.Build
    /// </summary>
    public static class CookingSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Cooking.unity";
        private static readonly Vector3 BowlCenter = new Vector3(-3.13f, 0.0f, 0f);
        private static readonly Vector3 TrayCenter = new Vector3(4.26f, 0.03f, 0f);

        // kind, tray position, tray rotation (deg), scale when landed in the bowl
        private static readonly (IngredientKind kind, string file, Vector2 pos, float rot, float landed)[] Items =
        {
            (IngredientKind.Flour,  "flour_pile",     new Vector2(4.80f,  2.60f),   0f, 1.30f),
            (IngredientKind.Butter, "butter_curl",    new Vector2(4.97f,  0.75f), -20f, 1.00f),
            (IngredientKind.Flour,  "flour_pile",     new Vector2(2.88f, -0.49f),   0f, 1.30f),
            (IngredientKind.Butter, "butter_curl",    new Vector2(2.83f,  1.65f),  12f, 1.00f),
            (IngredientKind.Sugar,  "sugar_crystals", new Vector2(5.06f, -1.26f),   0f, 1.00f),
            (IngredientKind.Butter, "butter_curl",    new Vector2(3.91f, -2.75f),   8f, 1.00f),
        };

        [MenuItem("NoriVerse/Build Cooking Scene")]
        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = CreateCamera(22.4f, new Color32(255, 222, 92, 255));   // yellow table fills any extra room
            var root = new GameObject("CookingRoot").transform;

            Sprite(root, "Background", "Kitchen/Cooking/cook_bg.png", Vector3.zero, 0);
            var back = Sprite(root, "BackButton", "Kitchen/Menu/btn_back.png", new Vector3(-9.78f, 4.1f, 0), 40);
            back.gameObject.AddComponent<ScreenCornerAnchor>().Configure(cam);
            back.gameObject.AddComponent<CircleCollider2D>().radius = 0.58f;
            back.gameObject.AddComponent<NoriVerse.Gameplay.PizzaFlow.SceneBackButton>().Configure(cam);

            // ---- bowl, batter, drop zone -------------------------------------------------------------------------
            Sprite(root, "Bowl", "Kitchen/Cooking/bowl.png", BowlCenter, 10);
            var bowlArea = Child(root, "BowlArea", BowlCenter).gameObject.AddComponent<BowlArea>();
            bowlArea.Configure(3.3f);

            var stageRoot = Child(root, "BatterStages", BowlCenter);
            var stages = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
                stages[i] = Sprite(stageRoot, "Batter" + (i + 1), $"Kitchen/Cooking/batter_{i + 1}.png", Vector3.zero, 22 + i);
            var batter = stageRoot.gameObject.AddComponent<BatterVisual>();
            batter.Configure(bowlArea, stageRoot, stages);

            // ---- tray + ingredients ---------------------------------------------------------------------------------
            Sprite(root, "Tray", "Kitchen/Cooking/tray.png", TrayCenter, 10);
            var items = new Ingredient[Items.Length];
            for (int i = 0; i < Items.Length; i++)
            {
                var it = Items[i];
                var sr = Sprite(root, $"{it.kind}_{i + 1}", $"Kitchen/Cooking/{it.file}.png", new Vector3(it.pos.x + 0.35f, it.pos.y, 0), 20);
                sr.transform.localRotation = Quaternion.Euler(0, 0, it.rot);
                float radius = Mathf.Min(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y) * 0.9f;
                AddCircle(sr.gameObject, radius);
                var ing = sr.gameObject.AddComponent<Ingredient>();
                ing.Configure(it.kind, it.landed);
                items[i] = ing;
            }

            // ---- touch dot, hint hand, whisk, stir hint ---------------------------------------------------------------
            var dot = Child(root, "TouchDot", Vector3.zero).gameObject.AddComponent<SpriteRenderer>();
            dot.sprite = RuntimeSprites.Circle();
            dot.color = new Color(0.45f, 0.45f, 0.5f, 0f);
            dot.sortingOrder = 190;
            dot.transform.localScale = Vector3.one * 0.55f;

            var dragController = root.gameObject.AddComponent<IngredientDragController>();
            dragController.Configure(cam, bowlArea, items, dot);

            var handRoot = Child(root, "HintHand", Vector3.zero);
            var hand = Sprite(handRoot, "Hand", "Kitchen/Cooking/hand_point.png", Vector3.zero, 160);
            var ghost = Child(handRoot, "Ghost").gameObject.AddComponent<SpriteRenderer>();
            ghost.sortingOrder = 150;
            var ripple = Child(handRoot, "TapRipple").gameObject.AddComponent<SpriteRenderer>();
            ripple.sprite = RuntimeSprites.Circle();
            ripple.color = new Color(1f, 1f, 1f, 0f);
            ripple.sortingOrder = 140;
            var hint = handRoot.gameObject.AddComponent<HintHand>();
            hint.Configure(hand, ghost, ripple);

            var whiskSr = Sprite(root, "Whisk", "Kitchen/Cooking/whisk.png", BowlCenter + new Vector3(1.45f, 0.55f, 0), 100);
            var whisk = whiskSr.gameObject.AddComponent<WhiskController>();
            whisk.Configure(cam, bowlArea, whiskSr.transform, whiskSr);

            var arrowRoot = Child(root, "StirHint", BowlCenter);
            var arrow = arrowRoot.gameObject.AddComponent<StirHintArrow>();
            arrow.Configure(2.3f, new Color32(96, 190, 220, 255));

            var overlay = Overlay(root, 300);
            var controller = root.gameObject.AddComponent<CookingController>();
            controller.Configure(cam, overlay, bowlArea, dragController, hint, whisk, arrow, batter);

            EditorSceneManager.MarkSceneDirty(scene);
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScenes(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("NoriVerse: cooking scene built at " + ScenePath);
        }
    }
}
