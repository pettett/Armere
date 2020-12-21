using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Armere.Inventory;
public class ItemSpawner : Spawner
{
    public ItemName item;

    public static async Task<ItemSpawnable> SpawnItemAsync(ItemName item, Vector3 position, Quaternion rotation)
    {
        var go = ((PhysicsItemData)InventoryController.singleton.db[item]).gameObject;

        Assert.IsTrue(go.RuntimeKeyIsValid(), $"No gameobject reference for {item}");

        ItemSpawnable spawnable = (ItemSpawnable)await GameObjectSpawner.SpawnAsync(go, position, rotation);
        spawnable.Init(item, 1);
        return spawnable;
    }

    public override async Task<SpawnableBody> Spawn()
    {
        return await SpawnItemAsync(item, transform.position, transform.rotation);
    }

    private async void Start()
    {
        await Spawn();
    }
}