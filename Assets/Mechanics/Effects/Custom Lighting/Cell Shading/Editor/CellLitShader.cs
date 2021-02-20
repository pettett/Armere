using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.Rendering;

public class CellLitShader : BaseShaderGUI
{

	public static GUIContent workflowModeText = new GUIContent("Workflow Mode",
		"Select a workflow that fits your textures. Choose between Metallic or Specular.");

	public static GUIContent specularMapText =
		new GUIContent("Specular Map", "Sets and configures the map and color for the Specular workflow.");

	public static GUIContent metallicMapText =
		new GUIContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");

	public static GUIContent smoothnessText = new GUIContent("Smoothness",
		"Controls the spread of highlights and reflections on the surface.");

	public static GUIContent smoothnessMapChannelText =
		new GUIContent("Source",
			"Specifies where to sample a smoothness map from. By default, uses the alpha channel for your map.");

	public static GUIContent highlightsText = new GUIContent("Specular Highlights",
		"When enabled, the Material reflects the shine from direct lighting.");

	public static GUIContent reflectionsText =
		new GUIContent("Environment Reflections",
			"When enabled, the Material samples reflections from the nearest Reflection Probes or Lighting Probe.");

	public static GUIContent heightMapText = new GUIContent("Height Map",
		"Specifies the Height Map (G) for this Material.");

	public static GUIContent occlusionText = new GUIContent("Occlusion Map",
		"Sets an occlusion map to simulate shadowing from ambient lighting.");

	public static GUIContent shadowCutoffText = new GUIContent("Shadow Cutoff", "");

	public static GUIContent fresnelCutoffText = new GUIContent("Fresnel Cutoff", "");

	public static GUIContent fresnelMultiplierText = new GUIContent("Fresnel Multiplier", "");

	public static readonly string[] metallicSmoothnessChannelNames = { "Metallic Alpha", "Albedo Alpha" };
	public static readonly string[] specularSmoothnessChannelNames = { "Specular Alpha", "Albedo Alpha" };

	public static GUIContent clearCoatText = new GUIContent("Clear Coat",
		"A multi-layer material feature which simulates a thin layer of coating on top of the surface material." +
		"\nPerformance cost is considerable as the specular component is evaluated twice, once per layer.");

	public static GUIContent clearCoatMaskText = new GUIContent("Mask",
		"Specifies the amount of the coat blending." +
		"\nActs as a multiplier of the clear coat map mask value or as a direct mask value if no map is specified." +
		"\nThe map specifies clear coat mask in the red channel and clear coat smoothness in the green channel.");

	public static GUIContent clearCoatSmoothnessText = new GUIContent("Smoothness",
		"Specifies the smoothness of the coating." +
		"\nActs as a multiplier of the clear coat map smoothness value or as a direct smoothness value if no map is specified.");

	public MaterialProperty bumpMapProp;
	public MaterialProperty bumpScaleProp;
	public MaterialProperty smoothness;
	public MaterialProperty metallic;
	public MaterialProperty metallicGlossMap;
	public MaterialProperty shadowCutoff;
	public MaterialProperty fresnelCutoff;
	public MaterialProperty fresnelMultiplier;
	public MaterialProperty smoothnessMapChannel;
	public override void FindProperties(MaterialProperty[] properties)
	{
		base.FindProperties(properties);
		bumpMapProp = BaseShaderGUI.FindProperty("_BumpMap", properties);
		bumpScaleProp = BaseShaderGUI.FindProperty("_BumpScale", properties);

		smoothness = BaseShaderGUI.FindProperty("_Smoothness", properties);
		metallic = BaseShaderGUI.FindProperty("_Metallic", properties);
		metallicGlossMap = BaseShaderGUI.FindProperty("_MetallicGlossMap", properties);

		shadowCutoff = BaseShaderGUI.FindProperty("_ShadowCutoff", properties);
		fresnelCutoff = BaseShaderGUI.FindProperty("_FresnelCutoff", properties);
		fresnelMultiplier = BaseShaderGUI.FindProperty("_FresnelMultiplier", properties);

		smoothnessMapChannel = BaseShaderGUI.FindProperty("_SmoothnessTextureChannel", properties, false);


	}
	public override void DrawSurfaceInputs(Material material)
	{
		base.DrawSurfaceInputs(material);
		BaseShaderGUI.DrawNormalArea(materialEditor, bumpMapProp, bumpScaleProp);

		DoMetallicSpecularArea(materialEditor, material);

		DoCellShading(material);

	}

	public override void MaterialChanged(Material material)
	{
		BaseShaderGUI.SetMaterialKeywords(material, SetMaterialKeywords);
	}

	public void DoSlider(MaterialProperty prop, GUIContent text)
	{
		EditorGUI.showMixedValue = prop.hasMixedValue;
		var m = EditorGUILayout.Slider(text, prop.floatValue, 0f, 1f);
		if (EditorGUI.EndChangeCheck())
			prop.floatValue = m;
		EditorGUI.showMixedValue = false;
	}

	public void DoMetallicSpecularArea(MaterialEditor materialEditor, Material material)
	{
		string[] smoothnessChannelNames;
		bool hasGlossMap = false;

		hasGlossMap = metallicGlossMap.textureValue != null;
		smoothnessChannelNames = metallicSmoothnessChannelNames;
		materialEditor.TexturePropertySingleLine(metallicMapText, metallicGlossMap,
			hasGlossMap ? null : metallic);

		EditorGUI.indentLevel++;
		DoSmoothness(material, smoothnessChannelNames);
		EditorGUI.indentLevel--;
	}


	public void DoSmoothness(Material material, string[] smoothnessChannelNames)
	{
		var opaque = ((BaseShaderGUI.SurfaceType)material.GetFloat("_Surface") ==
					  BaseShaderGUI.SurfaceType.Opaque);
		EditorGUI.indentLevel++;
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = smoothness.hasMixedValue;
		var s = EditorGUILayout.Slider(smoothnessText, smoothness.floatValue, 0f, 1f);
		if (EditorGUI.EndChangeCheck())
			smoothness.floatValue = s;
		EditorGUI.showMixedValue = false;

		if (smoothnessMapChannel != null) // smoothness channel
		{
			EditorGUI.indentLevel++;
			EditorGUI.BeginDisabledGroup(!opaque);
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = smoothnessMapChannel.hasMixedValue;
			var smoothnessSource = (int)smoothnessMapChannel.floatValue;
			if (opaque)
				smoothnessSource = EditorGUILayout.Popup(smoothnessMapChannelText, smoothnessSource,
					smoothnessChannelNames);
			else
				EditorGUILayout.Popup(smoothnessMapChannelText, 0, smoothnessChannelNames);
			if (EditorGUI.EndChangeCheck())
				smoothnessMapChannel.floatValue = smoothnessSource;
			EditorGUI.showMixedValue = false;
			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;
	}

	public void DoCellShading(Material material)
	{
		DoSlider(shadowCutoff, shadowCutoffText);
		DoSlider(fresnelCutoff, fresnelCutoffText);
		DoSlider(fresnelMultiplier, fresnelMultiplierText);
	}


	public enum SmoothnessMapChannel
	{
		SpecularMetallicAlpha,
		AlbedoAlpha,
	}

	public static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
	{
		int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
		if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
			return SmoothnessMapChannel.AlbedoAlpha;

		return SmoothnessMapChannel.SpecularMetallicAlpha;
	}

	public static void SetMaterialKeywords(Material material)
	{
		// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
		// (MaterialProperty value might come from renderer material property block)
		var hasGlossMap = false;
		//var isSpecularWorkFlow = false;
		var opaque = ((BaseShaderGUI.SurfaceType)material.GetFloat("_Surface") ==
					  BaseShaderGUI.SurfaceType.Opaque);

		hasGlossMap = material.GetTexture("_MetallicGlossMap") != null;


		// CoreUtils.SetKeyword(material, "_SPECULAR_SETUP", isSpecularWorkFlow);

		CoreUtils.SetKeyword(material, "_METALLICSPECGLOSSMAP", hasGlossMap);

		if (material.HasProperty("_SpecularHighlights"))
			CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
				material.GetFloat("_SpecularHighlights") == 0.0f);
		if (material.HasProperty("_EnvironmentReflections"))
			CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
				material.GetFloat("_EnvironmentReflections") == 0.0f);
		// if (material.HasProperty("_OcclusionMap"))
		// 	CoreUtils.SetKeyword(material, "_OCCLUSIONMAP", material.GetTexture("_OcclusionMap"));

		// if (material.HasProperty("_ParallaxMap"))
		// 	CoreUtils.SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));

		if (material.HasProperty("_SmoothnessTextureChannel"))
		{
			CoreUtils.SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
				GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha && opaque);
		}

		// // Clear coat keywords are independent to remove possiblity of invalid combinations.
		// if (ClearCoatEnabled(material))
		// {
		// 	var hasMap = material.HasProperty("_ClearCoatMap") && material.GetTexture("_ClearCoatMap") != null;
		// 	if (hasMap)
		// 	{
		// 		CoreUtils.SetKeyword(material, "_CLEARCOAT", false);
		// 		CoreUtils.SetKeyword(material, "_CLEARCOATMAP", true);
		// 	}
		// 	else
		// 	{
		// 		CoreUtils.SetKeyword(material, "_CLEARCOAT", true);
		// 		CoreUtils.SetKeyword(material, "_CLEARCOATMAP", false);
		// 	}
		// }
		// else
		// {
		// 	CoreUtils.SetKeyword(material, "_CLEARCOAT", false);
		// 	CoreUtils.SetKeyword(material, "_CLEARCOATMAP", false);
		// }

	}
}
