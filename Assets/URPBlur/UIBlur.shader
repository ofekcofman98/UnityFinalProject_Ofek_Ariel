Shader "Hidden/Custom/Blur"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _BlurSize("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay" }

        Pass
        {
            Name "BlurPass"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float _BlurSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
                half4 col = 0;

                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + texelSize * float2(-1.0, -1.0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + texelSize * float2(-1.0,  1.0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + texelSize * float2( 1.0, -1.0));
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + texelSize * float2( 1.0,  1.0));
                col *= 0.25;

                return col;
            }

            ENDHLSL
        }
    }
}
