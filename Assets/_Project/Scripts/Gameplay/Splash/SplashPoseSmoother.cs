using UnityEngine;

namespace NoriVerse.Gameplay.Splash
{
    /// <summary>Retains the original splash artwork, blending poses with constant body opacity and eased timing.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SplashPoseSmoother:MonoBehaviour
    {
        [SerializeField] private Shader blendShader;
        private SpriteRenderer source;
        private Sprite target;
        private Material material,originalMaterial;
        private float elapsed,duration;
        public bool IsBlending=>duration>0&&elapsed<duration;
        public void Configure(Shader shader)=>blendShader=shader;
        private void Awake()
        {
            source=GetComponent<SpriteRenderer>();target=source.sprite;originalMaterial=source.sharedMaterial;
            if(blendShader==null)blendShader=Shader.Find("NoriVerse/Splash Pose Blend");
            material=new Material(blendShader);material.SetTexture("_PreviousTex",target.texture);material.SetFloat("_Blend",1);source.sharedMaterial=material;
        }
        private void LateUpdate()
        {
            if(source.sprite!=target)
            {
                string a=target!=null?target.name:"",b=source.sprite!=null?source.sprite.name:"";
                material.SetTexture("_PreviousTex",target.texture);target=source.sprite;elapsed=0;
                bool cropOnly=a.Replace("_cut","")==b.Replace("_cut","");
                duration=cropOnly?0:b.Contains("blink")||a.Contains("blink")?.10f:.26f;
            }
            elapsed+=Time.deltaTime;
            float k=duration<=0?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration));
            material.SetFloat("_Blend",k);
        }
        private void OnDestroy()
        {if(source!=null)source.sharedMaterial=originalMaterial;if(material!=null)Destroy(material);}
    }
}
