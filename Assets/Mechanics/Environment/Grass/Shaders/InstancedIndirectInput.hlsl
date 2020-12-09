#ifndef GRASS_INSTANCED_LIT_INPUT_INCLUDED
#define GRASS_INSTANCED_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "MatrixStruct.cginc"

CBUFFER_START(UnityPerMaterial)
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
<<<<<<< HEAD
StructuredBuffer<MatrixStruct> _Properties;
=======
float4 _BaseMap_ST;
float viewDistance = 5;
float distanceFading = 1;
>>>>>>> 50588ef6582aa230e035005b7444fbed58347fd2
CBUFFER_END



#endif