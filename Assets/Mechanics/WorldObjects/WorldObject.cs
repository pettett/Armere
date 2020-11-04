using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public readonly struct WorldObjectInstanceData
{
    public readonly ItemName containsItem;
    public readonly uint containsItemCount;

    public WorldObjectInstanceData(ItemName containsItem, uint containsItemCount)
    {
        this.containsItem = containsItem;
        this.containsItemCount = containsItemCount;
    }
}


public class WorldObject : MonoBehaviour
{
    public WorldObjectInstanceData instanceData;

    public AsyncOperationHandle<GameObject> prefabHandle;

    public void AddContentsToInventory()
    {
        InventoryController.AddItem(instanceData.containsItem, instanceData.containsItemCount);
    }
    public async Task SpawnContentsToWorld()
    {
        WorldObjectData d = await Addressables.LoadAssetAsync<WorldObjectData>(((PhysicsItemData)InventoryController.singleton.db[instanceData.containsItem]).worldObjectData).Task;

        Task<WorldObject>[] t = new Task<WorldObject>[instanceData.containsItemCount];
        for (int i = 0; i < instanceData.containsItemCount; i++)
        {
            t[i] = WorldObjectSpawner.SpawnWorldObjectAsync(d, transform.position, transform.rotation, new WorldObjectInstanceData(instanceData.containsItem, 1));
        }

        await Task.WhenAll(t);

        Addressables.Release(d);
    }
}
