using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class ChestSpawner : ItemSpawner
{

    public AssetReferenceGameObject chest;
    public uint containerCount = 1;

    public override async Task<SpawnableBody> Spawn()
    {
        SpawnableBody spawnable = await GameObjectSpawner.SpawnAsync(chest, transform.position, transform.rotation);
        ((ItemSpawnable)spawnable).Init(item, containerCount);
        return spawnable;
    }
}