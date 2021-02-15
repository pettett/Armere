using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[ExecuteAlways]
public class SkyboxController : MonoBehaviour
{
	RenderTexture[] skyboxRenderCubemaps;
	Cubemap skyboxCubemap;

	public int widthPower = 7;
	public ComputeShader skyboxCompute;
	public float skyboxUpdateThreshold = 0.1f;

	// Start is called before the first frame update
	private void Start()
	{
		skyboxRenderCubemaps = new RenderTexture[6];

		for (int i = 0; i < 6; i++)
		{
			skyboxRenderCubemaps[i] = new RenderTexture(new RenderTextureDescriptor(1 << widthPower, 1 << widthPower));
			//skyboxRenderCubemap.dimension = TextureDimension.Cube;
			//skyboxRenderCubemaps.dimension = TextureDimension.Tex2DArray;
			//skyboxRenderCubemaps.volumeDepth = 6;

			skyboxRenderCubemaps[i].enableRandomWrite = true;
			skyboxRenderCubemaps[i].Create();

			skyboxCompute.SetTexture(i, "Result", skyboxRenderCubemaps[i]);
			skyboxCompute.SetInt("width", 1 << widthPower);
		}




		skyboxCubemap = new Cubemap(1 << widthPower, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

		RenderSettings.customReflection = skyboxCubemap;

		RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
	}

	// Update is called once per frame
	private void OnDestroy()
	{

		DestroyImmediate(skyboxCubemap, false);
		for (int i = 0; i < 6; i++)
		{
			DestroyImmediate(skyboxRenderCubemaps[i], false);
		}


		RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
	}
	Vector4 lastColor;


	void OnBeginCameraRendering(ScriptableRenderContext context, Camera[] camera)
	{
		if (TimeDayController.singleton == null) return;

		//Get current skybox color and compare it to before
		Vector4 currentCol = TimeDayController.singleton.GetSkyColor();

		if ((lastColor - currentCol).sqrMagnitude > skyboxUpdateThreshold)
		{
			//Update the skybox
			lastColor = currentCol;

			CommandBuffer buffer = CommandBufferPool.Get();



			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_DaySkyColor, TimeDayController.singleton.daySkyColour);
			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_NightSkyColor, TimeDayController.singleton.nightSkyColour);
			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SkyColorTransitionPeriod, TimeDayController.singleton.skyColorTransitionPeriod);

			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunDir, -RenderSettings.sun.transform.forward);
			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunCoTangent, -RenderSettings.sun.transform.up);
			buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunTangent, -RenderSettings.sun.transform.right);
			int width = 1 << (widthPower - 3);

			for (int i = 0; i < 6; i++)
			{
				//2^width / 8 = 2^width / 2^3 = 2^(width-3)
				buffer.DispatchCompute(skyboxCompute, i, width, width, 1);
			}



			for (int i = 0; i < 6; i++)
				buffer.CopyTexture(skyboxRenderCubemaps[i], 0, skyboxCubemap, i);



			context.ExecuteCommandBuffer(buffer);

			CommandBufferPool.Release(buffer);
		}
	}
}
