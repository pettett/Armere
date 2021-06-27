using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLayerData : MonoBehaviour
{
	[System.Serializable]
	public struct TerrainLayer
	{
		[ColorUsage(false, false)] public Color color;
		public Texture2D colorGradient;
	}
	public TerrainLayer[] terrainLayers = new TerrainLayer[4];
}
