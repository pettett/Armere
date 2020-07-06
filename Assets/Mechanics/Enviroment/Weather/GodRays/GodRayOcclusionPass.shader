Shader "Hidden/Custom/GodRayOcclusionPass"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture); 


        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoord).r;
            float occusion =  1- ceil(depth);
            return occusion;
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