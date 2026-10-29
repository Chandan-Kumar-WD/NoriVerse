using System.Collections.Generic;
using NoriVerse.Characters;
using NoriVerse.Gameplay.Kitchen;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static NoriVerse.EditorTools.SceneBuilderUtil;

namespace NoriVerse.EditorTools
{
    /// <summary>
    /// Builds the kitchen menu scene (Video 002, 0-17 s): the pot on white, then the kitchen with Nori behind the counter and
    /// seven dishes. Layout numbers come from Docs/Kitchen_Video_002_Breakdown.md (Unity units, camera size 5).
    /// Run via NoriVerse/Build Kitchen Scene or -executeMethod NoriVerse.EditorTools.KitchenSceneBuilder.Build
    /// </summary>
    public static class KitchenSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Kitchen.unity";

        private static readonly (string food, float y)[] Dishes =
        {
            ("pancakes", 0), ("cheese_toast", 0), ("pear_tart", 0), ("tomato_pizza", 0), ("cookie_pizza", 0), ("star_fruit", 0), ("veggie_pizza", 0)
        };
        private static readonly float[] DishX = { -9.38f, -6.26f, -3.12f, 0f, 3.14f, 6.26f, 9.38f };
        private const float DishY = -3.65f, CounterTop = -1.44f;
        private const int UnlockedDish = 3;                       // the round pizza in the middle starts the cooking
        private const float BustBottomY = -2.05f;                 // her chest cut hangs well below the counter edge, hidden behind it

        [MenuItem("NoriVerse/Build Kitchen Scene")]
        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // the whole 22 u wide design must always be visible; the camera colour fills any extra room on taller screens
            var cam = CreateCamera(22.4f, new Color32(107, 208, 212, 255));
            var root = new GameObject("KitchenRoot").transform;

            // ---- kitchen background ------------------------------------------------------------------------
            var wall = Sprite(root, "Wall", "Kitchen/Menu/wall.png", Vector3.zero, 0);
            wall.transform.localScale = new Vector3(1f, 1.5f, 1f);       // taller, so tablets (4:3) see wall all the way up
            var herbShelf = Sprite(root, "HerbShelf", "Kitchen/Menu/herb_wall_shelf.png", new Vector3(-8.5f, 1.25f, 0), 5);
            herbShelf.transform.localScale = new Vector3(1f, 0.95f / herbShelf.sprite.bounds.size.y, 1f);
            Sprite(root, "HerbPots", "Kitchen/Menu/herb_pots.png", new Vector3(-8.5f, 1.55f, 0), 7);
            Sprite(root, "Window", "Kitchen/Menu/window.png", new Vector3(9.96f, 1.45f, 0), 5);
            Sprite(root, "Counter", "Kitchen/Menu/counter.png", new Vector3(0, CounterTop, 0), 20);
            Fill(root, "CounterFill", new Color32(252, 215, 99, 255), new Vector2(0, CounterTop - 7.5f), new Vector2(40f, 15f), 19);

            // ---- Nori behind the counter ----------------------------------------------------------------------
            // HeadPivot is the neck: moods rotate/bob around it; the bust hangs below it with its paws on the counter edge.
            var neck = Child(root, "NoriHeadPivot", new Vector3(0, -0.5f, 0));
            var bust = Child(neck, "NoriBust", new Vector3(0, BustBottomY + 0.5f, 0));   // neck sits 0.5 u below the origin
            var faces = bust.gameObject.AddComponent<SpriteCrossfader>();
            var poses = new List<SpriteCrossfader.Pose>();
            foreach (var (pose, file) in new[]
            {
                (NoriMoodDirector.Smile, "smile"), (NoriMoodDirector.Blink, "blink"), (NoriMoodDirector.HalfBlink, "half_blink"),
                (NoriMoodDirector.BigSmile, "big_smile"), (NoriMoodDirector.Worried, "worried"), (NoriMoodDirector.WorriedLeft, "worried_left"),
                (NoriMoodDirector.WorriedRight, "worried_right"), (NoriMoodDirector.WorriedClosed, "worried_closed"),
                (NoriMoodDirector.SurprisedArms, "surprised_arms"), (NoriMoodDirector.Yawn, "yawn"), (NoriMoodDirector.Sleepy, "sleepy"),
                (NoriMoodDirector.ArmsHalf, "arms_half"), (NoriMoodDirector.YawnHalf, "yawn_half"), (NoriMoodDirector.SmileWide, "smile_wide")
            })
                poses.Add(new SpriteCrossfader.Pose { name = pose, sprite = LoadSprite($"Characters/Nori/bust/nori_bust_{file}.png") });
            foreach(var expression in new[]{("anticipate","nori_anticipate"),("open","nori_bite_open"),("bite","nori_bite_close"),("chew_left","nori_chew_left"),("chew_right","nori_chew_right"),("satisfied","nori_savour")})
                poses.Add(new SpriteCrossfader.Pose{name="eat_"+expression.Item1,sprite=LoadSprite($"PizzaUpgrade/{expression.Item2}.png")});
            faces.Configure(poses, 15, NoriMoodDirector.Smile);          // behind the counter (order 20)

            // her resting paws are separate sprites drawn in front of the counter, planted on its edge
            var paws = new List<SpriteRenderer>();
            foreach (var side in new[] { "L", "R" })
            {
                float px = side == "L" ? -1.32f : 1.32f;
                paws.Add(Sprite(neck, "CounterPaw" + side, $"Characters/Nori/bust/nori_bust_paw{side}.png", new Vector3(px, -1.07f, 0), 25));
            }
            var mood = neck.gameObject.AddComponent<NoriMoodDirector>();
            mood.Configure(faces, neck, paws.ToArray());

            // ---- dishes -------------------------------------------------------------------------------------------
            var dishButtons = new List<DishButton>();
            for (int i = 0; i < Dishes.Length; i++)
            {
                var dish = Child(root, "Dish" + (i + 1) + "_" + Dishes[i].food, new Vector3(DishX[i], DishY, 0));
                var visual = Child(dish, "Visual");
                // Each illustration includes food and its plate in a coherent serving perspective.
                // Preserve its original proportions: volume comes from the artwork, never Y compression.
                var shadow = Sprite(visual, "CounterContactShadow", "FX/ground_shadow.png", new Vector3(0.04f, -0.55f, 0), 23);
                shadow.transform.localScale = new Vector3(2.85f / shadow.sprite.bounds.size.x, 0.70f / shadow.sprite.bounds.size.y, 1f);
                shadow.color = new Color(0.28f, 0.19f, 0.08f, 0.32f);
                Sprite(visual, "ServedDish", $"Kitchen/Menu/served_{Dishes[i].food}.png", new Vector3(0, -0.85f, 0), 26);
                var padlock = Sprite(visual, "Padlock", "Kitchen/Menu/padlock.png", new Vector3(-1.0f, -0.10f, 0), 28).gameObject;
                padlock.transform.localScale = Vector3.one * 0.63f;
                var hit = dish.gameObject.AddComponent<BoxCollider2D>();
                hit.size = new Vector2(3.0f, 2.6f);
                hit.offset = new Vector2(0, 0.25f);
                var button = dish.gameObject.AddComponent<DishButton>();
                button.Configure(i != UnlockedDish, visual, padlock);   // only the centre pizza can be cooked
                dishButtons.Add(button);
            }

            // ---- pot + white cover (the intro on white) ------------------------------------------------------------------
            var overlay = Overlay(root, 50);
            var pot = Child(root, "Pot", new Vector3(0, -0.3f, 0));
            var potBody = Sprite(pot, "PotBody", "Kitchen/Pot/pot_body.png", Vector3.zero, 60);
            var lid = Child(pot, "LidPivot", new Vector3(0.1f, 1.0f, 0));
            var potLid = Sprite(lid, "PotLid", "Kitchen/Pot/pot_lid.png", new Vector3(0, 0.1f, 0), 61);

            var controller = root.gameObject.AddComponent<KitchenMenuController>();
            controller.Configure(overlay, pot, lid, new[] { potBody, potLid }, mood, dishButtons.ToArray(), cam);
            var snapshotSource=PizzaFinishSceneBuilder.CreateVisual(root,"ServingSnapshotSource",new Vector3(50,0,0),1,100);
            snapshotSource.gameObject.SetActive(false);
            var meal=Child(root,"HeldCookedPizza").gameObject.AddComponent<SpriteRenderer>();meal.sortingOrder=120;Tween.SetAlpha(meal,0);
            var sun=Sprite(root,"MealSunCloud","PizzaFinish/sun_cloud.png",new Vector3(3,2.7f,0),65);Tween.SetAlpha(sun,0);
            var reactions=Child(root,"MealReactions").gameObject.AddComponent<MealReactionFx>();reactions.Configure(sun);
            var serving=root.gameObject.AddComponent<PizzaServingController>();
            serving.Configure(cam,controller,dishButtons[UnlockedDish],meal,snapshotSource,mood,faces,neck,PizzaFlowSceneBuilder.Hint(root,cam),reactions);
            controller.AttachServing(serving);

            EditorSceneManager.MarkSceneDirty(scene);
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterScenes(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("NoriVerse: kitchen scene built at " + ScenePath);
        }
    }
}
