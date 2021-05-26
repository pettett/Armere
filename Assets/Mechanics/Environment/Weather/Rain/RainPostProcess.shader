Shader "Hidden/RainPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex FullScreenTrianglePostProcessVertexProgram
            #pragma fragment Frag


			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

			#include "noiseSimplex.hlsl"

        	TEXTURE2D_X(_MainTex); 
			SAMPLER(sampler_MainTex);
			
			TEXTURE2D_X(_NoiseTex); 
			SAMPLER(sampler_NoiseTex);

			uniform float _RainEdgeHeight;
			uniform float _RainDepth;
			uniform float _RainDensity;
			uniform float4 _RainColor;

			// Instead of recieving the vertex position, we just receive a vertex id (0,1,2)
			// and convert it to a clip-space postion in the vertex shader
			struct FullScreenTrianglePostProcessAttributes
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// This is what the fragment program should recieve if you use "FullScreenTrianglePostProcessVertexProgram" as the vertex shader
			struct PostProcessVaryings
			{
				float4 positionCS : SV_POSITION;
				float2 texcoord   : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			PostProcessVaryings FullScreenTrianglePostProcessVertexProgram(FullScreenTrianglePostProcessAttributes input)
			{
				PostProcessVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
				output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
				return output;
			}

			float4 Frag(PostProcessVaryings i) : SV_Target
			{


				float3 normalValues1 = SampleSceneNormals(i.texcoord);
				
				float depth1 = SampleSceneDepth(i.texcoord);
				float3 worldNormal = mul((float3x3)unity_CameraToWorld, normalValues1);
				float2 uv2 = i.texcoord - float2(0, _RainEdgeHeight * (1.1-depth1) * (1 -worldNormal.y));
				float3 normalValues2 = SampleSceneNormals(uv2);




				float depth2 = SampleSceneDepth(uv2);


				float noise = SAMPLE_TEXTURE2D_X(_NoiseTex,sampler_NoiseTex, (i.texcoord * (depth1 + 2)) + round(_Time.x * 750) * .1f).r;

				float4 col = SAMPLE_TEXTURE2D_X(_MainTex,sampler_MainTex,i.texcoord);

				if (depth1 > depth2 * _RainDepth && worldNormal.y > .5f) {
					return step(noise.r, _RainDensity) * _RainColor + col;
				}else{
                	return col;
				}

            }
            ENDHLSL
        }
    }
}
