using System.Collections.Generic;
using NoriVerse.Gameplay.PizzaFinish;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static NoriVerse.EditorTools.SceneBuilderUtil;
using static NoriVerse.EditorTools.PizzaFlowSceneBuilder;

namespace NoriVerse.EditorTools
{
    public static class PizzaFinishSceneBuilder
    {
        public const string DecoratePath="Assets/_Project/Scenes/PizzaDecorate.unity";
        public const string BakePath="Assets/_Project/Scenes/PizzaBake.unity";
        private static string ArtFile(string name)
        {
            if(name=="oven_body")return "PizzaUpgrade/oven_premium.png";
            if(name=="oven_rack")return "PizzaUpgrade/oven_rack_premium.png";
            return "PizzaFinish/"+name+".png";
        }
        private static string FlowFile(string name)
        {
            if(name=="prep_board")return "PizzaUpgrade/prep_board_premium.png";
            if(name=="salami_slice")return "PizzaUpgrade/salami_clean.png";
            return "PizzaFlow/"+name+".png";
        }
        public static Sprite[] ToppingSprites()=>new[]{LoadSprite(FlowFile("tomato_slice")),LoadSprite(FlowFile("cheese_piece")),LoadSprite(FlowFile("salami_slice")),LoadSprite(FlowFile("basil_sprig"))};
        public static PizzaVisual CreateVisual(Transform parent,string name,Vector3 position,float scale,int order)
        {
            var t=Child(parent,name,position);t.localScale=Vector3.one*scale;
            var raw=Sprite(t,"RawCrust",FlowFile("pizza_base"),Vector3.zero,order);
            var cooked=Sprite(t,"BakedCrust",ArtFile("baked_base"),Vector3.zero,order+1);Tween.SetAlpha(cooked,0);
            var red=Sprite(t,"Sauce",FlowFile("sauce_disc"),Vector3.zero,order+2);
            var p=t.gameObject.AddComponent<PizzaVisual>();
            p.Configure(raw,cooked,red,Child(t,"Cheese"),Child(t,"Toppings"),LoadSprite(FlowFile("cheese_shred")),ToppingSprites(),order);return p;
        }
        [MenuItem("NoriVerse/Build Pizza Finish")]
        public static void BuildAll(){AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);BuildDecorate();BuildBake();}
        [MenuItem("NoriVerse/Run Pizza Finish")]
        public static void RunFinish(){EditorSceneManager.OpenScene(DecoratePath);EditorApplication.EnterPlaymode();}
        public static void BuildDecorate()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=CreateCamera(22.4f,new Color32(255,225,107,255));var root=new GameObject("PizzaDecorateRoot").transform;
            var bg=Sprite(root,"YellowWorktop",FlowFile("yellow_background"),Vector3.zero,0);bg.gameObject.AddComponent<ScreenCoverFitter>();
            Sprite(root,"PrepBoard",FlowFile("prep_board"),Vector3.zero,5);
            var pizza=CreateVisual(root,"PlayerPizza",new Vector3(-0.85f,0,0),1,15);
            var tray=Sprite(root,"ToppingPalette","Kitchen/Cooking/tray.png",new Vector3(5.15f,1.45f,0),35);
            // Reuse the handle-free tall tray so the entire palette stays inside the view.
            tray.transform.localScale=new Vector3(2.75f/tray.sprite.bounds.size.x,7.05f/tray.sprite.bounds.size.y,1);
            var sources=ToppingSprites();float[] ys={-0.95f,0.65f,3.9f,2.25f};var palette=new SpriteRenderer[4];
            for(int i=0;i<4;i++)
            {
                var sr=Child(root,"Palette_"+(ToppingKind)i,new Vector3(5.15f,ys[i],0)).gameObject.AddComponent<SpriteRenderer>();sr.sprite=sources[i];sr.sortingOrder=40;palette[i]=sr;
            }
            var confirm=Check(root,"BakeConfirm",new Vector2(5,-3.55f),2.1f);
            var controller=root.gameObject.AddComponent<PizzaDecorationController>();controller.Configure(cam,pizza,palette,confirm,Overlay(root,250),Hint(root,cam));
            Back(root,cam);Save(DecoratePath);
        }
        public static void BuildBake()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=CreateCamera(22.4f,new Color32(60,211,207,255));var root=new GameObject("PizzaBakeRoot").transform;
            var bg=Sprite(root,"TurquoiseWorktop",ArtFile("oven_background"),Vector3.zero,0);bg.gameObject.AddComponent<ScreenCoverFitter>();
            var body=Sprite(root,"OvenBody",ArtFile("oven_body"),Vector3.zero,10);body.transform.localScale=new Vector3(1,9.4f/body.sprite.bounds.size.y,1);
            var carriage=Child(root,"RackCarriage",new Vector3(0,-9,0));
            Sprite(carriage,"SingleOvenRack",ArtFile("oven_rack"),new Vector3(-0.85f,0,0),15);
            var pizza=CreateVisual(carriage,"BakingPizza",new Vector3(-0.85f,0,0),0.86f,20);
            var glint=Shape(root,"GlassGlintLeft",RuntimeSprites.Capsule(),new Vector2(-3.2f,1.2f),new Vector2(5.5f,0.32f),new Color(1,1,1,0.09f),45);glint.transform.localRotation=Quaternion.Euler(0,0,45);
            glint=Shape(root,"GlassGlintRight",RuntimeSprites.Capsule(),new Vector2(0.8f,-1.5f),new Vector2(4.5f,0.25f),new Color(1,1,1,0.07f),45);glint.transform.localRotation=Quaternion.Euler(0,0,45);
            var power=Child(root,"OvenPowerButton",new Vector3(4.45f,0.1f,0));
            Shape(power,"ButtonRim",RuntimeSprites.RoundedRectangle(1.12f,1.85f,0.26f),Vector2.zero,new Vector2(1.12f,1.85f),new Color32(57,119,12,255),60);
            Shape(power,"GreenButton",RuntimeSprites.RoundedRectangle(0.98f,1.7f,0.23f),Vector2.zero,new Vector2(0.98f,1.7f),new Color32(80,217,20,255),61);
            Shape(power,"PowerShine",RuntimeSprites.RoundedRectangle(.44f,.06f,.03f),new Vector2(-.1f,.71f),new Vector2(.44f,.06f),new Color(1,1,1,.55f),62);
            Shape(power,"PowerSymbol",FinishSymbols.Power(),new Vector2(0,0.03f),new Vector2(0.82f,0.82f),Color.white,62);
            var plus=Shape(root,"PlusDecoration",RuntimeSprites.Capsule(),new Vector2(4.45f,1.65f),new Vector2(0.38f,0.08f),new Color(0.21f,0.06f,0.03f,0.5f),50);
            plus=Shape(root,"PlusVertical",RuntimeSprites.Capsule(),new Vector2(4.45f,1.65f),new Vector2(0.38f,0.08f),new Color(0.21f,0.06f,0.03f,0.5f),50);plus.transform.localRotation=Quaternion.Euler(0,0,90);
            Shape(root,"MinusDecoration",RuntimeSprites.Capsule(),new Vector2(4.45f,-1.5f),new Vector2(0.38f,0.08f),new Color(0.21f,0.06f,0.03f,0.5f),50);
            Shape(root,"HeatBarRim",RuntimeSprites.RoundedRectangle(7.6f,0.56f,0.28f),new Vector2(-0.8f,4.35f),new Vector2(7.6f,0.56f),new Color32(255,249,210,255),60);
            Shape(root,"HeatBarEmpty",RuntimeSprites.RoundedRectangle(7.1f,0.3f,0.15f),new Vector2(-0.8f,4.35f),new Vector2(7.1f,0.3f),new Color32(208,158,62,255),61);
            var fill=Shape(root,"HeatProgress",RuntimeSprites.RoundedRectangle(7.1f,0.3f,0.15f),new Vector2(-0.8f,4.35f),new Vector2(7.1f,0.3f),new Color32(130,235,50,255),62);
            Shape(fill.transform,"FillShine",RuntimeSprites.RoundedRectangle(6.8f,.045f,.0225f),new Vector2(0,.075f),new Vector2(6.8f,.045f),new Color(1,1,.6f,.55f),63);
            var flame=Child(root,"HeatFlame",new Vector3(2.9f,4.35f,0));
            Shape(flame,"FlameBadge",RuntimeSprites.Circle(),Vector2.zero,new Vector2(0.86f,0.86f),new Color32(255,249,210,255),63);
            Shape(flame,"Flame",FinishSymbols.Flame(),Vector2.zero,new Vector2(0.58f,0.63f),Color.white,64);
            var hint=Hint(root,cam);Back(root,cam);
            var reward=CreateReward(root,cam);
            var controller=root.gameObject.AddComponent<PizzaBakingController>();controller.Configure(cam,carriage,power,flame,Overlay(root,900),fill,pizza,reward,hint);
            Save(BakePath);
        }
        private static PizzaRewardController CreateReward(Transform root,Camera cam)
        {
            var dim=Fill(root,"RewardDimmer",new Color32(0,0,0,0),Vector2.zero,new Vector2(40,24),500);
            dim.gameObject.AddComponent<ScreenCoverFitter>();
            var plate=Child(root,"RewardPlateRoot",new Vector3(-0.5f,-0.45f,0));
            var rim=Sprite(plate,"BlueRimmedPlate","Kitchen/Menu/dish_plate.png",Vector3.zero,610);rim.transform.localScale=Vector3.one*(4.1f/2.9f);
            var hero=CreateVisual(plate,"FinishedPlayerPizza",Vector3.zero,0.43f,620);
            var roots=new Transform[3];var fills=new SpriteRenderer[3];var glows=new SpriteRenderer[3];
            Vector2[] positions={new Vector2(-3,2.8f),new Vector2(-0.5f,3.7f),new Vector2(2,2.8f)};
            for(int i=0;i<3;i++)
            {
                roots[i]=Child(root,"RewardStar"+i,positions[i]);
                glows[i]=Shape(roots[i],"Glow",RuntimeSprites.Circle(),Vector2.zero,new Vector2(2.4f,2.4f),new Color(1,0.79f,0.13f,0),700);
                var empty=Sprite(roots[i],"EmptyStar","PizzaUpgrade/reward_star_empty.png",Vector3.zero,702);empty.color=new Color(.72f,.66f,.57f,1);
                fills[i]=Sprite(roots[i],"GoldStar","PizzaUpgrade/reward_star_gold.png",Vector3.zero,703);Tween.SetAlpha(fills[i],0);
            }
            var avatar=Child(root,"RewardNoriPortrait");var bounds=avatar.gameObject.AddComponent<SpriteRenderer>();bounds.sprite=LoadSprite(ArtFile("avatar_smile"));bounds.color=Color.clear;
            Shape(avatar,"WhiteRing",RuntimeSprites.Circle(),Vector2.zero,new Vector2(3,3),Color.white,650);
            Shape(avatar,"TurquoiseCircle",RuntimeSprites.Circle(),Vector2.zero,new Vector2(2.88f,2.88f),new Color32(141,249,237,255),651);
            var face=Child(avatar,"PortraitExpressions").gameObject.AddComponent<SpriteCrossfader>();
            face.Configure(new List<SpriteCrossfader.Pose>{new SpriteCrossfader.Pose{name="smile",sprite=LoadSprite(ArtFile("avatar_smile"))},new SpriteCrossfader.Pose{name="blink",sprite=LoadSprite(ArtFile("avatar_blink"))},new SpriteCrossfader.Pose{name="satisfied",sprite=LoadSprite(ArtFile("avatar_satisfied"))}},660,"smile");
            avatar.gameObject.AddComponent<ScreenCornerAnchor>().Configure(cam,0.7f,true);
            var burst=Child(root,"RewardSparkles").gameObject.AddComponent<SuccessBurst>();burst.ConfigureSorting(750);
            var paper=Child(root,"RewardConfetti").gameObject.AddComponent<ConfettiBurst>();
            var reward=root.gameObject.AddComponent<PizzaRewardController>();reward.Configure(dim,plate,hero,avatar,face,roots,fills,glows,burst,paper);
            plate.gameObject.SetActive(false);avatar.gameObject.SetActive(false);foreach(var t in roots)t.gameObject.SetActive(false);return reward;
        }
    }
}
