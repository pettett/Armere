using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "Fluid Template", menuName = "Game/Fluids/Fluid Template", order = 0)]
public class FluidTemplate : ScriptableObject
{
	public AudioClip[] smallSplashes;
	public AudioClip[] mediumSplashes;
	public AudioClip[] largeSplashes;


	public AssetReferenceGameObject splashEffectPrefab;
	//Default values of water
	public float density = 1000, viscosity = 1;


	FluidTypeInstance _sceneInstance;
	public FluidTypeInstance sceneInstance
	{
		get
		{
			if (_sceneInstance == null)
			{
				_sceneInstance = new GameObject($"{name} Instance", typeof(FluidTypeInstance)).GetComponent<FluidTypeInstance>();
				_sceneInstance.template = this;
			}
			return _sceneInstance;

		}
	}

}