using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RainRenderPassFeature : ScriptableRendererFeature
{
	sealed class RainRenderPass : ScriptableRenderPass
	{
		public RainSettings settings;
		Material material;

		// This method is called before executing the render pass.
		// It can be used to configure render targets and their clear state. Also to create temporary render target textures.
		// When empty this render pass will render to the active camera render target.
		// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
		// The render pipeline will ensure target setup and clearing happens in a performant manner.
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			material = CoreUtils.CreateEngineMaterial("Hidden/RainPostProcess");
			material.SetTexture("_NoiseTex", settings.noiseTex);
			material.SetFloat("_RainEdgeHeight", settings.rainEdgeHeight);
			material.SetFloat("_RainDensity", settings.rainDensity);
			material.SetFloat("_RainDepth", settings.rainDepth);
			material.SetColor("_RainColor", settings.rainColor);
		}

		// Here you can implement the rendering logic.
		// Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
		// https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
		// You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get();
			cmd.Clear();


			CoreUtils.DrawFullScreen(cmd, material);


			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		// Cleanup any allocated resources that were created during the execution of this render pass.
		public override void OnCameraCleanup(CommandBuffer cmd)
		{
		}
	}
	[System.Serializable]
	public class RainSettings
	{
		public Texture2D noiseTex;
		public float rainEdgeHeight;
		public float rainDensity;
		public float rainDepth;
		public Color rainColor;
	}
	public RainSettings settings;

	RainRenderPass m_ScriptablePass;

	/// <inheritdoc/>
	public override void Create()
	{
		m_ScriptablePass = new RainRenderPass();
		m_ScriptablePass.settings = settings;

		// Configures where the render pass should be injected.
		m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
	}

	// Here you can inject one or multiple render passes in the renderer.
	// This method is called when setting up the renderer once per-camera.
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_ScriptablePass);
	}
}


