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

	public async void SetClothing(ClothesVariation variation)
	{
		await SetClothing(variation.clothes.position, variation.clothes.hideBody, variation.clothes.skinnedMesh, variation.clothes.clothes);
		if (variation.clothes.variations.Length > 0)
		{
			Renderer r = clothingHandles[(int)variation.clothes.position].Result.GetComponentInChildren<Renderer>();
			Assert.IsNotNull(r);
			r.materials = variation.clothes.variations[variation.variation].materials;
		}
	}

	public async Task SetClothing(ClothPosition clothingIndex, bool hideBody, bool skinned, AssetReferenceGameObject reference)
	{
		AsyncOperationHandle<GameObject> oldHandle = clothingHandles[(int)clothingIndex];

		clothingHandles[(int)clothingIndex] = Addressables.InstantiateAsync(reference, transform);
		if (skinned)
			LinkSkinnedMesh(await clothingHandles[(int)clothingIndex].Task);
		else
			(await clothingHandles[(int)clothingIndex].Task).transform.SetParent(headTransform, false);
		//After this mesh exists

		bodyParts[(int)clothingIndex]?.SetActive(!hideBody);


		if (oldHandle.IsValid())
		{

			Addressables.ReleaseInstance(oldHandle);
		}
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
