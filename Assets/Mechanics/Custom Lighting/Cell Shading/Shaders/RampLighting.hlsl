#ifndef RAMP_LIGHTING_INCLUDED
#define RAMP_LIGHTING_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline half3 DiffuseAndSpecularFromMetallic (half3 albedo, half metallic,out half3 specColor, out half oneMinusReflectivity) {
    specColor = lerp(kDieletricSpec.rgb, albedo, metallic);
    oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    return albedo * oneMinusReflectivity;
}
half FresnelSchlick(float cosTheta, half3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}              
half DistributionGGX(half3 N, half3 H, half a) // calculate specular reflections
{
    half a2     = a*a;
    half NdotH  = max(dot(N, H), 0.0);
    half NdotH2 = NdotH*NdotH;
    
    half nom    = a2;
    half denom  = (NdotH2 * (a2 - 1.0) + 1.0);
    denom        = PI * denom * denom;
    return nom / denom;
}

half3 LightingRampedLambert(half3 lightColor, half3 lightDir, half3 normal,half shadowCuttoff)
{
    half NdotL = smoothstep(shadowCuttoff,shadowCuttoff+0.01, saturate(dot(normal, lightDir)));
    return lightColor * NdotL;
}


half3 RampedVertexLighting(float3 positionWS, half3 normalWS,half shadowCuttoff)
{
    half3 vertexLightColor = half3(0.0, 0.0, 0.0);

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    uint lightsCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightsCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, positionWS);
        half3 lightColor = light.color * light.distanceAttenuation;
        vertexLightColor += LightingRampedLambert(lightColor, light.direction, normalWS,shadowCuttoff);
    }
#endif

    return vertexLightColor;
}

//custom additional lights


half RampedAngleAttenuation(half3 spotDirection, half3 lightDirection, half2 spotAttenuation)
{
    // Spot Attenuation with a linear falloff can be defined as
    // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
    // This can be rewritten as
    // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
    // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
    // SdotL * spotAttenuation.x + spotAttenuation.y

    // If we precompute the terms in a MAD instruction
    half SdotL = dot(spotDirection, lightDirection);
    half atten =floor(saturate(SdotL * spotAttenuation.x + spotAttenuation.y)*2)*0.5;
    return atten;
}

Light GetAdditionalPerObjectRampedLight(int perObjectLightIndex, float3 positionWS)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    half4 lightOcclusionProbeInfo = _AdditionalLightsBuffer[perObjectLightIndex].occlusionProbeChannels;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * RampedAngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);
    light.color = color;

    // In case we're using light probes, we can sample the attenuation from the `unity_ProbesOcclusion`
#if defined(LIGHTMAP_ON) || defined(_MIXED_LIGHTING_SUBTRACTIVE)
    // First find the probe channel from the light.
    // Then sample `unity_ProbesOcclusion` for the baked occlusion.
    // If the light is not baked, the channel is -1, and we need to apply no occlusion.

    // probeChannel is the index in 'unity_ProbesOcclusion' that holds the proper occlusion value.
    int probeChannel = lightOcclusionProbeInfo.x;

    // lightProbeContribution is set to 0 if we are indeed using a probe, otherwise set to 1.
    half lightProbeContribution = lightOcclusionProbeInfo.y;

    half probeOcclusionValue = unity_ProbesOcclusion[probeChannel];
    light.distanceAttenuation *= max(probeOcclusionValue, lightProbeContribution);
#endif

    return light;
}
Light GetAdditionalRampedLight(uint i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    return GetAdditionalPerObjectRampedLight(perObjectLightIndex, positionWS);
}

#endif