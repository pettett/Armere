#ifndef CELL_LIT_INPUT_INCLUDED
#define CELL_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST; 
float4 _BumpMap_ST;
half4 _BaseColor;
half _BumpScale;
half _ShadowCutoff;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _FresnelCutoff;
half _FresnelMultiplier;
CBUFFER_END


//#ifdef _SPECULAR_SETUP
//    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
//#else
//    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
//#endif


#endif // CELL_INPUT_SURFACE_PBR_INCLUDED
