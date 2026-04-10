// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Source
// https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129
//
// EDIT: Added _Transparency property and edited frag -- now there's true transparency 
//

Shader "Custom/ColorSwap_URP"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _SwapTex("Color Data", 2D) = "transparent" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Transparency("Transparency", Range(0.0,1.0)) = 1.0
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "SpriteUnlit"
            Tags { "LightMode" = "Universal2D" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            float4 _Color;
            float _Transparency;
            sampler2D _MainTex;
            sampler2D _SwapTex;
            float4 _MainTex_ST;
            
            // Para compatibilidad con el sistema de sprites de Unity
            sampler2D _AlphaTex;
            float _AlphaSplitEnabled;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            half4 SampleSpriteTexture(float2 uv)
            {
                half4 color = tex2D(_MainTex, uv);
                if (_AlphaSplitEnabled)
                    color.a = tex2D(_AlphaTex, uv).r;
                return color;
            }

            half4 frag(v2f IN) : SV_Target
            {
                half4 c = SampleSpriteTexture(IN.texcoord);
                half4 swapCol = tex2D(_SwapTex, half2(c.r, 0));
                half4 final = lerp(c, swapCol, swapCol.a) * IN.color;
                
                if (c.a < 1) {
                    final.a = c.a;
                    final.rgb *= c.a;
                }
                else {
                    final.a = _Transparency;
                    final.rgb *= _Transparency;
                }
                return final;
            }
            ENDHLSL
        }
    }
}