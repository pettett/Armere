#ifndef CELL_LIT_INPUT_INCLUDED
#define CELL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST; 
float4 _BumpMap_ST;
float4 _MetallicGlossMap_ST;
half4 _BaseColor;
half _BumpScale;
half _ShadowCutoff;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _FresnelCutoff;
half _FresnelMultiplier;
CBUFFER_END


TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;


    specGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv);
    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif

	specGloss.r *= _Metallic;


    return specGloss;
}


//#ifdef _SPECULAR_SETUP
//    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
//#else
//    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
//#endif


#endif // CELL_INPUT_SURFACE_PBR_INCLUDED
