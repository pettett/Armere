using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterMeshController : MonoBehaviour
{

    public SkinnedMeshRenderer mainCharacterMesh;


    readonly AsyncOperationHandle<GameObject>[] clothingHandles = new AsyncOperationHandle<GameObject>[3];



    public async void SetClothing(int clothingIndex, AssetReferenceGameObject reference)
    {
        AsyncOperationHandle<GameObject> oldHandle = clothingHandles[clothingIndex];

        clothingHandles[clothingIndex] = Addressables.InstantiateAsync(reference, transform);

        LinkSkinnedMesh(await clothingHandles[clothingIndex].Task);

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
