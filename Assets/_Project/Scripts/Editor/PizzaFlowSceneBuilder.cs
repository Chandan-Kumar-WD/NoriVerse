using System.Collections.Generic;
using NoriVerse.Gameplay.PizzaFlow;
using NoriVerse.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static NoriVerse.EditorTools.SceneBuilderUtil;

namespace NoriVerse.EditorTools
{
    public static class PizzaFlowSceneBuilder
    {
        public const string DoughPath="Assets/_Project/Scenes/Dough.unity";
        public const string PrepPath="Assets/_Project/Scenes/PizzaPrep.unity";
        public const string SortPath="Assets/_Project/Scenes/ToppingSort.unity";
        private static string ArtFile(string name)
        {
            if(name=="prep_board")return "PizzaUpgrade/prep_board_premium.png";
            if(name=="salami_slice")return "PizzaUpgrade/salami_clean.png";
            if(name=="salami_slice_silhouette")return "PizzaUpgrade/salami_clean_silhouette.png";
            return "PizzaFlow/"+name+".png";
        }
        [MenuItem("NoriVerse/Build Pizza Flow")]
        public static void BuildAll(){AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);BuildDough();BuildPrep();BuildSort();}
        [MenuItem("NoriVerse/Run Dough")]
        public static void RunDough(){EditorSceneManager.OpenScene(DoughPath);EditorApplication.EnterPlaymode();}
        public static void Save(string path)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),path);RegisterScenes(path);AssetDatabase.SaveAssets();Debug.Log("NoriVerse: built "+path);
        }
        public static void Back(Transform root,Camera cam)
        {
            root.gameObject.AddComponent<TouchFeedback>().Configure(cam);
            var back=Sprite(root,"BackButton","Kitchen/Menu/btn_back.png",Vector3.zero,200);
            AddCircle(back.gameObject,0.58f);back.gameObject.AddComponent<SceneBackButton>().Configure(cam);
            back.gameObject.AddComponent<ScreenCornerAnchor>().Configure(cam);
        }
        public static GestureHint Hint(Transform root,Camera cam)
        {
            var t=Child(root,"GestureHint");
            var hand=Sprite(t,"Hand","Kitchen/Cooking/hand_point.png",Vector3.zero,180);Tween.SetAlpha(hand,0);
            var ghost=Child(t,"Ghost").gameObject.AddComponent<SpriteRenderer>();ghost.sortingOrder=170;Tween.SetAlpha(ghost,0);
            var h=t.gameObject.AddComponent<GestureHint>();h.Configure(cam,hand,ghost);return h;
        }
        public static SpriteRenderer Shape(Transform parent,string name,Sprite shape,Vector2 pos,Vector2 size,Color color,int order)
        {
            var sr=Child(parent,name,pos).gameObject.AddComponent<SpriteRenderer>();sr.sprite=shape;sr.sortingOrder=order;sr.color=color;
            sr.transform.localScale=new Vector3(size.x/shape.bounds.size.x,size.y/shape.bounds.size.y,1);return sr;
        }
        public static Transform Check(Transform root,string name,Vector2 pos,float scale=1)
        {
            var t=Child(root,name,pos);t.localScale=Vector3.one*scale;
            Shape(t,"ButtonShadow",RuntimeSprites.RoundedRectangle(1.14f,.76f,.31f),new Vector2(0,-.035f),new Vector2(1.14f,.76f),new Color32(42,145,36,255),83);
            Shape(t,"GreenButton",RuntimeSprites.RoundedRectangle(1.12f,.72f,.30f),Vector2.zero,new Vector2(1.12f,.72f),new Color32(39,192,44,255),84);
            Shape(t,"ButtonFace",RuntimeSprites.RoundedRectangle(1.02f,.61f,.25f),new Vector2(0,.025f),new Vector2(1.02f,.61f),new Color32(103,242,69,255),85);
            Shape(t,"ButtonShine",RuntimeSprites.RoundedRectangle(.23f,.025f,.0125f),new Vector2(-.27f,.245f),new Vector2(.23f,.025f),new Color(1,1,1,.75f),86);
            Vector2 a=new Vector2(-.28f,.005f),b=new Vector2(-.075f,-.18f),c=new Vector2(.30f,.20f);
            CheckStroke(t,"CheckShort",a,b,.105f,Color.white,87);
            CheckStroke(t,"CheckLong",b,c,.105f,Color.white,87);
            return t;
        }
        private static void CheckStroke(Transform parent,string name,Vector2 from,Vector2 to,float thickness,Color color,int order)
        {
            float length=Vector2.Distance(from,to);
            var line=Shape(parent,name,RuntimeSprites.RoundedRectangle(length+thickness,thickness,thickness*.5f),(from+to)*.5f,new Vector2(length+thickness,thickness),color,order);
            line.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(to.y-from.y,to.x-from.x)*Mathf.Rad2Deg);
        }
        public static void BuildDough()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=CreateCamera(22.4f,new Color32(246,215,169,255));var root=new GameObject("DoughRoot").transform;
            var wood=Sprite(root,"WoodTable",ArtFile("wood_background"),Vector3.zero,0);wood.transform.localScale=new Vector3(1,10.5f/wood.sprite.bounds.size.y,1);
            var board=Sprite(root,"RollingBoard",ArtFile("rolling_board"),Vector3.zero,5);board.transform.localScale=new Vector3(1,9.35f/board.sprite.bounds.size.y,1);
            var small=Sprite(root,"SmallDough",ArtFile("dough_small"),Vector3.zero,12);
            var middle=Sprite(root,"MediumDough",ArtFile("dough_medium"),Vector3.zero,13);Tween.SetAlpha(middle,0);
            var sheet=Sprite(root,"RolledSheet",ArtFile("dough_rolled"),Vector3.zero,14);Tween.SetAlpha(sheet,0);
            var round=Sprite(root,"CutRoundBase",ArtFile("pizza_base"),new Vector3(0,-0.1f,0),16);round.transform.localScale=Vector3.one*(6.8f/8.45f);Tween.SetAlpha(round,0);
            var pin=Sprite(root,"RollingPin",ArtFile("rolling_pin"),new Vector3(0,7,0),30);
            var hint=Hint(root,cam);var burst=root.gameObject.AddComponent<SuccessBurst>();
            var roll=root.gameObject.AddComponent<DoughRollingController>();roll.Configure(cam,small,middle,sheet,pin,hint);
            var cut=root.gameObject.AddComponent<DoughCutController>();cut.Configure(cam,hint);
            var controller=root.gameObject.AddComponent<DoughSceneController>();
            controller.Configure(Overlay(root,250),round,new[]{small,middle,sheet},pin.transform,roll,cut,burst);
            Back(root,cam);Save(DoughPath);
        }
        public static void BuildPrep()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=CreateCamera(22.4f,new Color32(255,225,107,255));var root=new GameObject("PizzaPrepRoot").transform;
            var bg=Sprite(root,"YellowWorktop",ArtFile("yellow_background"),Vector3.zero,0);bg.transform.localScale=new Vector3(1.2f,1.65f,1);
            Sprite(root,"CreamPrepBoard",ArtFile("prep_board"),Vector3.zero,5);
            Sprite(root,"PizzaBase",ArtFile("pizza_base"),new Vector3(-0.85f,0,0),10);
            var sauce=Sprite(root,"TomatoSauce",ArtFile("sauce_disc"),new Vector3(-0.85f,0,0),15);
            // Generated ceramic ramekins supply real illustrated depth, not a flat UI badge.
            Vector2 badge=new Vector2(5.15f,3.65f);
            var red=Sprite(root,"SauceIcon","PizzaUpgrade/sauce_holder.png",badge,44);
            var yellow=Sprite(root,"CheeseIcon","PizzaUpgrade/cheese_holder.png",badge,45);Tween.SetAlpha(yellow,0);
            var hint=Hint(root,cam);var burst=root.gameObject.AddComponent<SuccessBurst>();
            var paint=root.gameObject.AddComponent<SaucePaintController>();paint.Configure(cam,sauce);
            var cheese=root.gameObject.AddComponent<CheeseSprinkleController>();cheese.Configure(cam,LoadSprite(ArtFile("cheese_shred")),Child(root,"CheesePieces"));
            var confirm=Check(root,"ConfirmCheese",new Vector2(5.0f,-3.55f),2.1f);confirm.gameObject.SetActive(false);
            var prep=root.gameObject.AddComponent<PizzaPrepController>();prep.Configure(cam,Overlay(root,250),red,yellow,paint,cheese,hint,burst,confirm);
            Back(root,cam);Save(PrepPath);
        }
        private static string IngredientFile(ToppingKind kind)
        {
            switch(kind){case ToppingKind.Tomato:return "tomato_slice";case ToppingKind.Cheese:return "cheese_piece";case ToppingKind.Salami:return "salami_slice";default:return "basil_sprig";}
        }
        public static void BuildSort()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=CreateCamera(22.4f,new Color32(255,225,107,255));var root=new GameObject("ToppingSortRoot").transform;
            Fill(root,"YellowExtension",new Color32(255,225,107,255),Vector2.zero,new Vector2(40,40),-3);
            Fill(root,"TurquoiseExtension",new Color32(36,205,201,255),new Vector2(0,8.8f),new Vector2(40,20),-2);
            Sprite(root,"UpperStripedTable",ArtFile("sort_upper"),new Vector3(0,-1.2f,0),0);
            Sprite(root,"LowerYellowTable",ArtFile("sort_lower"),new Vector3(0,-1.2f,0),0);
            var tray=Sprite(root,"ToppingTray",ArtFile("sorting_tray"),new Vector3(0,2.0f,0),10);tray.transform.localScale=new Vector3(1,5.1f/tray.sprite.bounds.size.y,1);
            var items=new List<SortIngredient>();
            var positions=new[]{new Vector2(-4.3f,3.25f),new Vector2(-2.45f,3.35f),new Vector2(-1.35f,3.45f),new Vector2(0.2f,3.1f),new Vector2(1.4f,3.45f),new Vector2(3.85f,3.2f),new Vector2(-4.1f,1.95f),new Vector2(-2.5f,2.05f),new Vector2(-1.1f,2.45f),new Vector2(2.5f,2.8f),new Vector2(1.35f,1.65f),new Vector2(4f,1.7f),new Vector2(-4.5f,0.8f),new Vector2(-3f,1f),new Vector2(-1.55f,0.75f),new Vector2(0f,1f),new Vector2(2.65f,0.85f),new Vector2(3.7f,0.7f)};
            var kinds=new[]{ToppingKind.Cheese,ToppingKind.Salami,ToppingKind.Cheese,ToppingKind.Basil,ToppingKind.Salami,ToppingKind.Salami,ToppingKind.Salami,ToppingKind.Tomato,ToppingKind.Tomato,ToppingKind.Basil,ToppingKind.Tomato,ToppingKind.Tomato,ToppingKind.Basil,ToppingKind.Cheese,ToppingKind.Cheese,ToppingKind.Cheese,ToppingKind.Salami,ToppingKind.Tomato};
            for(int i=0;i<positions.Length;i++)
            {
                var sr=Sprite(root,"TrayIngredient_"+i+"_"+kinds[i],ArtFile(IngredientFile(kinds[i])),positions[i],20+i);
                sr.transform.localRotation=Quaternion.Euler(0,0,(i%5-2)*9f);
                AddCircle(sr.gameObject,0.65f);
                var item=sr.gameObject.AddComponent<SortIngredient>();item.Configure(kinds[i],sr);items.Add(item);
            }
            var targets=new List<SortTarget>();
            for(int i=0;i<4;i++)
            {
                var kind=(ToppingKind)i;var t=Child(root,"Target_"+kind,new Vector3((i-1.5f)*3.3f,-3.3f,0));
                Sprite(t,"Plate","Kitchen/Menu/dish_plate.png",Vector3.zero,45);
                var grey=Sprite(t,"Silhouette",ArtFile(IngredientFile(kind)+"_silhouette"),Vector3.zero,46);grey.color=new Color(0.65f,0.70f,0.71f,0.65f);
                var label=Sprite(t,"CategoryLabel",ArtFile("prep_board"),new Vector3(0,2.15f,0),50);label.transform.localScale=new Vector3(2.8f/label.sprite.bounds.size.x,1.05f/label.sprite.bounds.size.y,1);
                var icon=Sprite(t,"LabelIcon",ArtFile(IngredientFile(kind)),new Vector3(-0.60f,2.15f,0),51);icon.transform.localScale=Vector3.one*0.70f;
                Shape(t,"EmptyCheck",RuntimeSprites.Circle(),new Vector2(0.85f,2.15f),new Vector2(0.36f,0.36f),new Color(0.75f,0.78f,0.76f),51);
                var mark=Check(t,"CorrectCheck",new Vector2(0.85f,2.15f),0.62f);mark.gameObject.SetActive(false);
                var target=t.gameObject.AddComponent<SortTarget>();target.Configure(kind,grey,mark);targets.Add(target);t.localScale=Vector3.zero;
            }
            var controller=root.gameObject.AddComponent<ToppingSortController>();controller.Configure(cam,Overlay(root,250),items.ToArray(),targets.ToArray(),Hint(root,cam));
            Back(root,cam);Save(SortPath);
        }
    }
}
