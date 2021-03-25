using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterMeshController : MonoBehaviour
{
	public enum ClothPosition { Head, Chest, Legs }
	public SkinnedMeshRenderer mainCharacterMesh;

	public GameObject legsObject;
	public GameObject chestObject;
	public GameObject headObject;

	public Transform headTransform;
	readonly GameObject[] bodyParts = new GameObject[3];



	readonly AsyncOperationHandle<GameObject>[] clothingHandles = new AsyncOperationHandle<GameObject>[3];

	public void Start()
	{
		bodyParts[2] = legsObject;
		bodyParts[1] = chestObject;
		bodyParts[0] = headObject;
	}

	public void SetClothing(ClothesVariation variation)
	{
		var handle = SetClothing(variation.clothes.position, variation.clothes.hideBody, variation.clothes.skinnedMesh, variation.clothes.clothes);
		GameObjectSpawner.OnDone(handle, x =>
		{
			if (variation.clothes.variations.Length > 0)
			{
				Renderer r = x.Result.GetComponentInChildren<Renderer>();
				Assert.IsNotNull(r);
				r.materials = variation.clothes.variations[variation.variation].materials;
			}
		}
		);
	}

	public AsyncOperationHandle<GameObject> SetClothing(ClothPosition clothingIndex, bool hideBody, bool skinned, AssetReferenceGameObject reference)
	{
		AsyncOperationHandle<GameObject> oldHandle = clothingHandles[(int)clothingIndex];

		var newHandle = Addressables.InstantiateAsync(reference, transform);
		if (skinned)
			GameObjectSpawner.OnDone(newHandle, x => LinkSkinnedMesh(x.Result));
		else
			GameObjectSpawner.OnDone(newHandle, x => x.Result.transform.SetParent(headTransform, false));

		clothingHandles[(int)clothingIndex] = newHandle;
		//After this mesh exists

		bodyParts[(int)clothingIndex]?.SetActive(!hideBody);


		if (oldHandle.IsValid())
		{

			Addressables.ReleaseInstance(oldHandle);
		}

		return newHandle;
	}

	public void RemoveClothing(int clothingIndex)
	{
		if (clothingHandles[clothingIndex].IsValid())
		{
			Addressables.ReleaseInstance(clothingHandles[clothingIndex]);
		}
		bodyParts[clothingIndex]?.SetActive(true);
	}

	void LinkSkinnedMesh(GameObject mesh)
	{
		foreach (var t in mesh.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			t.bones = mainCharacterMesh.bones;
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < clothingHandles.Length; i++)
			if (clothingHandles[i].IsValid())
				Addressables.ReleaseInstance(clothingHandles[i]);

	}
}
