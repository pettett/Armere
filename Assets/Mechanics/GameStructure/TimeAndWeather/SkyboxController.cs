using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SkyboxController : MonoBehaviour
{
	RenderTexture[] skyboxRenderCubemaps;
	Cubemap skyboxCubemap;

	public int widthPower = 7;
	public ComputeShader skyboxCompute;
	public int skippedFrames = 10;

	// Start is called before the first frame update
	private void OnEnable()
	{
		skyboxRenderCubemaps = new RenderTexture[6];
		for (int i = 0; i < 6; i++)
		{
			skyboxRenderCubemaps[i] = new RenderTexture(new RenderTextureDescriptor(1 << widthPower, 1 << widthPower));
			//skyboxRenderCubemap.dimension = TextureDimension.Cube;
			//skyboxRenderCubemaps.dimension = TextureDimension.Tex2DArray;
			//skyboxRenderCubemaps.volumeDepth = 6;

			skyboxRenderCubemaps[i].enableRandomWrite = true;
		}




		skyboxCubemap = new Cubemap(1 << widthPower, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

		RenderSettings.customReflection = skyboxCubemap;

		RenderPipelineManager.beginFrameRendering += OnBeginCameraRendering;
	}

	// Update is called once per frame
	private void OnDisable()
	{

		Destroy(skyboxCubemap);
		for (int i = 0; i < 6; i++)
		{
			Destroy(skyboxRenderCubemaps[i]);
		}


		RenderPipelineManager.beginFrameRendering -= OnBeginCameraRendering;
	}
	int frame = 0;

	void OnBeginCameraRendering(ScriptableRenderContext context, Camera[] camera)
	{
		frame++;
		if (frame % skippedFrames == 0)
		{

			CommandBuffer buffer = CommandBufferPool.Get();

			int width = 1 << (widthPower - 3);

			for (int i = 0; i < 6; i++)
			{
				if (skyboxRenderCubemaps[i].IsCreated())
				{
					buffer.SetComputeTextureParam(skyboxCompute, 0, "Result", skyboxRenderCubemaps[i]);
					buffer.SetComputeIntParam(skyboxCompute, "face", i);
					buffer.SetComputeIntParam(skyboxCompute, "width", 1 << widthPower);

					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_DaySkyColor, TimeDayController.singleton.daySkyColour);
					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_NightSkyColor, TimeDayController.singleton.nightSkyColour);
					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SkyColorTransitionPeriod, TimeDayController.singleton.skyColorTransitionPeriod);

					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunDir, -RenderSettings.sun.transform.forward);
					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunCoTangent, -RenderSettings.sun.transform.up);
					buffer.SetComputeVectorParam(skyboxCompute, TimeDayController.shader_SunTangent, -RenderSettings.sun.transform.right);

					//2^width / 8 = 2^width / 2^3 = 2^(width-3)

					buffer.DispatchCompute(skyboxCompute, 0, width, width, 1);
				}
			}



			for (int i = 0; i < 6; i++)
				buffer.CopyTexture(skyboxRenderCubemaps[i], 0, skyboxCubemap, i);



			context.ExecuteCommandBuffer(buffer);

			CommandBufferPool.Release(buffer);
		}
	}
}
