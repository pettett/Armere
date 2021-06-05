
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "CloudInput.hlsl"
#include "Noise/SimplexNoise2D.hlsl"

struct Attributes
{
	float4 positionOS     : POSITION;
	float2 texcoord     : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float2 uv           : TEXCOORD0;
	float4 positionCS   : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};


float CloudStrength(float2 uv){
	float distFromCenter = saturate(length(uv - 0.5) * 2);

	float m = rcp(_DistanceWeightCutoffs.y - _DistanceWeightCutoffs.x);
	float c = - _DistanceWeightCutoffs.x * m;

	float distanceWeight = saturate(mad(m , distFromCenter , c));

	distanceWeight = 1- distanceWeight;
	distanceWeight *= distanceWeight;

	float noiseWeight =  (SimplexNoiseGrad(uv * _NoiseScale.xy).z + 1) * 0.5;

	return distanceWeight * noiseWeight;
}

Varyings CloudVertex(Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
	output.uv = input.texcoord;
	return output;
}

