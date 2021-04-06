using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.VFX;

public class FluidTypeInstance : MonoBehaviour
{
	public FluidTemplate template;

	AsyncOperationHandle<GameObject> splashEffectHandle;

	[System.NonSerialized] public VisualEffect splashEffect;

	private void Start()
	{
		splashEffectHandle = Addressables.InstantiateAsync(template.splashEffectPrefab, trackHandle: false);



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
		if (splashEffectHandle.IsValid() && splashEffectHandle.IsDone)
		{
			Addressables.ReleaseInstance(splashEffectHandle);
			splashEffect = null;
		}
	}
}
