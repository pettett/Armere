#ifndef GRASS_INSTANCED_LIT_INPUT_INCLUDED
#define GRASS_INSTANCED_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "MatrixStruct.hlsl"

CBUFFER_START(UnityPerMaterial)
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
float4 _BaseMap_ST;
StructuredBuffer<MatrixStruct> _Properties;
CBUFFER_END



#endif