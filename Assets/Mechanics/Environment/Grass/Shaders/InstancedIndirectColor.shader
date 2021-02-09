Shader "Custom/InstancedIndirectColor" {
	Properties{
		[MainColor] _BaseMap("Texture", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" 
				"RenderPipeline" = "UniversalPipeline"}

		Pass {
			//Cull Off
			Name "ForwardLit"
			Tags {"LightMode"= "UniversalForward"}
			// set cull mode inside subshader:
			Cull [_CullMode]

			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ALPHAPREMULTIPLY_ON

			#pragma shader_feature _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fog

			#pragma vertex vert
			#pragma fragment frag



			#include "InstancedIndirectInput.hlsl"
			
			struct appdata_t {



				float4 vertex   : POSITION;
				float2 texCoord : TEXCOORD0;
				//input normal and tangent for world space normals
				float3 normalOS     : NORMAL;
				float4 tangentOS    : TANGENT;
				float4 color    : COLOR;

			};

			struct v2f {
								
				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

				float4 vertex   : SV_POSITION;
				float2 texCoord : TEXCOORD0;
				float4 color    : COLOR;
				half fogFactor : TEXCOORD6;
				float3 posWS   : TEXCOORD2;    // xyz: posWS

				//If the main light uses shadows, store the coordinate on the shadow map
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD4;
				#endif
				//store the normal for lighting calculations
				float3 normalWS                 : TEXCOORD3;
				float3 viewDirectionWS : TEXCOORD5;

				UNITY_VERTEX_OUTPUT_STEREO
			}; 


			// UNITY_INSTANCING_BUFFER_START(Props)
			//     UNITY_DEFINE_INSTANCED_PROP(MatrixStruct, _Properties)
			// UNITY_INSTANCING_BUFFER_END(Props)


			v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {


				v2f o = (v2f)0;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);


				MatrixStruct p = _Properties[instanceID];

				//Apply matrix transformations
				float4 pos = mul(p.mat, i.vertex);

				float3 n = mul(p.mat, float4(i.normalOS,0)).xyz;
				float4 t = mul(p.mat, i.tangentOS);

				// float4 pos = i.vertex;
				// float3 n = i.normalOS;
				// float4 t = i.tangentOS;


 

				VertexPositionInputs vertexInput = GetVertexPositionInputs(pos.xyz);
				o.posWS = vertexInput.positionWS;
				VertexNormalInputs normalInput = GetVertexNormalInputs(n, t);

				half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
				o.viewDirectionWS = viewDirWS;
				o.vertex = vertexInput.positionCS;  
				o.color.rgb = p.color;
				o.color.a = 1;
				o.texCoord = i.texCoord;
				o.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				//store the shadow coordinate for lighting calculations
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					o.shadowCoord = GetShadowCoord(vertexInput);
				#endif

				o.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

				return o;
			}
			
			float4 frag(v2f i) : SV_Target {

				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				//Generate the color of the grass
				float4 diffuse = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap,i.texCoord) * i.color;

				clip(diffuse.a-0.5f);
				
				//Calculate the shadow coordinate for lighting
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord = i.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					float4 shadowCoord = TransformWorldToShadowCoord(i.posWS);
				#else
					float4 shadowCoord = float4(0, 0, 0, 0);
				#endif
 
				//get the main light for the Scene (using shadows if enabled)

				Light light = GetMainLight(shadowCoord);
				float3 normal = SafeNormalize(i.normalWS);
				//Use the information stored in light to render the grass

				float shadow = light.distanceAttenuation * light.shadowAttenuation;


				float3 diffuseLight = light.color * shadow ;

				half3 reflectVector = reflect(-i.viewDirectionWS, normal);

				// Add ambient/ GI lighting

				float3 bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, normal);

				MixRealtimeAndBakedGI(light, normal, bakedGI);

				float3 ambientLight = GlossyEnvironmentReflection(reflectVector,1,0.1) + bakedGI + unity_AmbientSky;

				float3 col = (ambientLight+diffuseLight) * diffuse.rgb;

				col = MixFog(col, i.fogFactor);
				return float4(col,1);
			}
			
			ENDHLSL
		}
		Pass{
			Name "ShadowCaster"
			Tags {"LightMode"= "ShadowCaster"}


			HLSLPROGRAM
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ALPHAPREMULTIPLY_ON
			#pragma shader_feature _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile_fog
			#pragma vertex vert
			#pragma fragment frag

			#include "InstancedIndirectInput.hlsl"


				half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
				{
					return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
				}


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			float3 _LightDirection;

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float2 texcoord     : TEXCOORD0;
			};

			struct Varyings
			{
				float2 uv           : TEXCOORD0;
				float4 positionCS   : SV_POSITION;
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

			#if UNITY_REVERSED_Z
				positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#else
				positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#endif

				return positionCS;
			}


			



			Varyings vert(Attributes input, uint instanceID: SV_InstanceID) {


				Varyings o = (Varyings)0;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//Apply matrix transformations
				input.positionOS = mul(_Properties[instanceID].mat, input.positionOS);

				o.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				o.positionCS = GetShadowPositionHClip(input);
				return o;
			}
			
			half4 frag(Varyings input) : SV_TARGET
			{

				clip(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a - 0.5f);
				return 0;
			}


			ENDHLSL
		}
	}
}