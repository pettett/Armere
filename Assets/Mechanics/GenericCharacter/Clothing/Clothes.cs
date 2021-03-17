using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName = "Clothes", menuName = "Game/Clothes")]

public class Clothes : ScriptableObject
{
	public CharacterMeshController.ClothPosition position;
	public AssetReferenceGameObject clothes;
	public bool skinnedMesh;
	public bool hideBody;

	[System.Serializable]
	public struct Variation
	{
		public Material[] materials;
	}
	public Variation[] variations;

}
[System.Serializable]
public struct ClothesVariation
{
	public Clothes clothes;
	public int variation;

}
