Shader "Hidden/Custom/VolumetricAdditivePass"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex); 
        TEXTURE2D_SAMPLER2D(_SampleMap, sampler_SampleMap); 

        float4 _MainTex_TexelSize;

        float3 _RayColor;

        float SampleTexture(float2 uv, float2 offset){
            uv += _MainTex_TexelSize * offset;
            return SAMPLE_TEXTURE2D(_SampleMap, sampler_SampleMap, uv).r;
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);



            float raySample = SampleTexture(i.texcoord,float2(0,0)) ;

             raySample += SampleTexture( i.texcoord, float2(1,0) ) ;
            raySample += SampleTexture( i.texcoord ,float2(-1,0));
            raySample += SampleTexture( i.texcoord, float2(0,1));
            raySample += SampleTexture( i.texcoord, float2(0,-1));

            raySample += SampleTexture( i.texcoord ,float2(1,1));
            raySample += SampleTexture( i.texcoord, float2(1,-1));
            raySample += SampleTexture( i.texcoord ,float2(-1,1));
            raySample += SampleTexture( i.texcoord ,float2(-1,-1));

            const float recip = rcp(9);
            raySample *= recip;

            return float4(raySample * _RayColor + color,1);
        }



    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}