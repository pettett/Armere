Shader "Hidden/Custom/VolumetricPostProcess"
{
    HLSLINCLUDE

        //#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    
        #include "noiseSimplex.hlsl"

        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE



        #define TEXTURE2D_SAMPLER2D(textureName, samplerName) Texture2D textureName; SamplerState samplerName


        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex); 



        float4x4  _CameraToWorld;
        uniform float4x4 _ClipToWorld;

        // x: scattering coef, y: extinction coef, z: range w: skybox extinction coef
		uniform float4 _VolumetricLight;

        // x: 1 - g^2, y: 1 + g^2, z: 2*g, w: 1/4pi
        uniform float4 _MieG;

        //XYZ, offset, W - scale
        uniform float4 _NoiseSettings;

        float _RenderViewportScaleFactor;

        float debug;



        struct AttributesDefault
        {
            float3 vertex : POSITION;
        };

        struct VaryingsDefault
        {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float2 texcoordStereo : TEXCOORD1;
        #if STEREO_INSTANCING_ENABLED
            uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
        #endif

            float3 direction : TEXCOORD2;

        };



        #if defined(UNITY_SINGLE_PASS_STEREO)
        float2 TransformStereoScreenSpaceTex(float2 uv, float w)
        {
            float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
            scaleOffset.xy *= _RenderViewportScaleFactor;
            return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
        }
        #else
        float2 TransformStereoScreenSpaceTex(float2 uv, float w)
        {
            return uv * _RenderViewportScaleFactor;
        }
        #endif



        // Vertex manipulation
        float2 TransformTriangleVertexToUV(float2 vertex)
        {
            float2 uv = (vertex + 1.0) * 0.5;
            return uv;
        }

        float3 GetDirection(float2 uv){
            // Invert the perspective projection of the view-space position
            return mul(_ClipToWorld, float4(uv, 0, 1.0f)) - _WorldSpaceCameraPos;
 
            // Transform the direction from camera to world space and normalize
           // direction = mul(_CameraToWorld, direction);
            //direction = normalize(direction);
           // return direction.xyz ;
        }

        VaryingsDefault VertDefault(AttributesDefault v)
        {
            VaryingsDefault o;
            o.vertex = float4(v.vertex.xy, 0.0, 1.0);
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

        #if UNITY_UV_STARTS_AT_TOP
            o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif

            o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

            // Transform pixel to [-1,1] range
            o.direction = GetDirection(o.texcoord * 2 - 1);        

            return o;
        }



       #define NUM_SAMPLES 32

		float MieScattering(float cosAngle, float4 g)
		{
            return g.w * (g.x / (pow(abs(g.y - g.z * cosAngle), 1.5)));			
		}

        float GetDensity(float3 wPos){
            return (snoise(mad(wPos ,_NoiseSettings.w, _NoiseSettings.xyz) ) + 1 )* 0.5;
        }

        float Frag(VaryingsDefault i) : SV_Target
        {
            float3 background = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord).rgb;

            // Transform the camera origin to world space
            float3 rayOrigin = mul(_CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

            float depth = SampleSceneDepth(i.texcoord);


            float3 rayDir = i.direction;
            //return float4(rayDir* 0.5f + 0.5f,1); //DEBUG - show world space directions


            float rayLength = LinearEyeDepth(depth,_ZBufferParams);

            Light light = GetMainLight();

            float total = 0;
            float extinction = 0;

            float recipricleSamples = rcp(NUM_SAMPLES);
            float cosAngle = dot(light.direction.xyz, -rayDir);


            [loop] for (uint index = 0; index < NUM_SAMPLES; index++){
                float distance = saturate(index * recipricleSamples) * rayLength;

                float3 samplePosWS = rayOrigin + rayDir * distance;


                float4 sampleShadowCoord =  TransformWorldToShadowCoord(samplePosWS);
                float atten = MainLightRealtimeShadow(sampleShadowCoord);

                float density = 1;// GetDensity(samplePosWS);

                float scattering = _VolumetricLight.x * recipricleSamples * density;
				extinction += _VolumetricLight.y * recipricleSamples * density;

                total += atten * scattering * exp(-extinction);
            }
           // total = light.shadowAttenuation;

            total = lerp(total,0,_VolumetricLight.w * (depth < 0.0001));

            total *= MieScattering(cosAngle, _MieG);
            return total;


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