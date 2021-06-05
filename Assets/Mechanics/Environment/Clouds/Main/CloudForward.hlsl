
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "CloudUtil.hlsl"


half4 CloudFragment(Varyings input) : SV_TARGET
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	half alpha = CloudStrength(input.uv);
	half4 diffuse = half4(1,1,1, alpha);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif

	return diffuse;
}