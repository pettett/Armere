using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.Assertions;

public class ChestSpawner : ItemSpawner
{

    public AssetReferenceGameObject chest;
    public uint containerCount = 1;
    public UnityEngine.Events.UnityEvent onChestOpened;
    public override async Task<SpawnableBody> Spawn()
    {
        Assert.IsTrue(chest.RuntimeKeyIsValid(), "Reference is null");
        SpawnableBody spawnable = await GameObjectSpawner.SpawnAsync(chest, transform.position, transform.rotation);
        ((ItemSpawnable)spawnable).Init(item, containerCount);
        ((InteractableChest)spawnable).onChestOpened += onChestOpened.Invoke;
        return spawnable;
    }
}