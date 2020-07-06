Shader "Hidden/Custom/GodRayAdditivePass"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex); 
        TEXTURE2D_SAMPLER2D(_SampleMap, sampler_SampleMap); 

        float3 _RayColor;


        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float raySample = SAMPLE_TEXTURE2D(_SampleMap, sampler_SampleMap, i.texcoord).r;
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