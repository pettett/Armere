#ifndef GRASS_INSTANCED_LIT_INPUT_INCLUDED
#define GRASS_INSTANCED_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


CBUFFER_START(UnityPerMaterial)
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;
float viewDistance = 5;
float distanceFading = 1;
CBUFFER_END

#endif