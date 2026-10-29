Shader "NoriVerse/Splash Pose Blend"
{
    Properties
    {
        [PerRendererData] _MainTex ("Current pose", 2D) = "white" {}
        _PreviousTex ("Previous pose", 2D) = "white" {}
        _Blend ("Pose blend", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex,_PreviousTex;
            float _Blend;
            Output vert(Input v) { Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            fixed4 frag(Output i):SV_Target
            {
                fixed4 a=tex2D(_PreviousTex,i.uv),b=tex2D(_MainTex,i.uv);
                fixed4 c=lerp(a,b,_Blend)*i.color;c.rgb*=c.a;return c;
            }
            ENDCG
        }
    }
}
