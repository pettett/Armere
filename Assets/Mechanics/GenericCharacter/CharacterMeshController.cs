using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterMeshController : MonoBehaviour
{

    public SkinnedMeshRenderer mainCharacterMesh;

    public GameObject legsObject;
    public GameObject chestObject;
    public GameObject headObject;

    readonly GameObject[] bodyParts = new GameObject[3];

    readonly AsyncOperationHandle<GameObject>[] clothingHandles = new AsyncOperationHandle<GameObject>[3];

    public void Start()
    {
        bodyParts[2] = legsObject;
        bodyParts[1] = chestObject;
        bodyParts[0] = headObject;
    }


    public async void SetClothing(int clothingIndex, bool hideBody, AssetReferenceGameObject reference)
    {
        AsyncOperationHandle<GameObject> oldHandle = clothingHandles[clothingIndex];

        clothingHandles[clothingIndex] = Addressables.InstantiateAsync(reference, transform);

        LinkSkinnedMesh(await clothingHandles[clothingIndex].Task);

        //After this mesh exists

        bodyParts[clothingIndex]?.SetActive(!hideBody);


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
