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

	AsyncOperationHandle<GameObject> splashEffectHandle;

	[System.NonSerialized] public VisualEffect splashEffect;
	public float density = 1000;
	private void OnEnable()
	{
		splashEffectHandle = Addressables.InstantiateAsync(splashEffectPrefab, trackHandle: false);
		if (splashEffectHandle.IsDone)
		{
			splashEffect = splashEffectHandle.Result.GetComponent<VisualEffect>();
		}
		else
		{
			splashEffectHandle.Completed += (x) => splashEffect = x.Result.GetComponent<VisualEffect>();
		}
	}
	private void OnDestroy()
	{
		Addressables.ReleaseInstance(splashEffectHandle);
	}
}