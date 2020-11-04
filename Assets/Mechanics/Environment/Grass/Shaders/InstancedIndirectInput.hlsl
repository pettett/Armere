#ifndef GRASS_INSTANCED_LIT_INPUT_INCLUDED
#define GRASS_INSTANCED_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


CBUFFER_START(UnityPerMaterial)
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
CBUFFER_END

#endif