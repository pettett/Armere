using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Assertions;

public class GameObjectSpawner : Spawner
{
    public static async Task<SpawnableBody> SpawnAsync(AssetReferenceGameObject gameObject, Vector3 position, Quaternion rotation, Transform parent = null)
    {

        var handle = Addressables.InstantiateAsync(gameObject, position, rotation, parent, false);
        var body = (await handle.Task).GetComponent<SpawnableBody>();

        Assert.IsNotNull(body, $"{handle.Result.name} has no SpawnableBody component");

        body.prefabHandle = handle;
        return body;
    }

    public static async Task<SpawnableBody> SpawnAsync(AssetReferenceGameObject gameObject, Transform parent = null, bool instantiateInWorldSpace = false)
    {
        var handle = Addressables.InstantiateAsync(gameObject, parent, instantiateInWorldSpace, false);
        var body = (await handle.Task).GetComponent<SpawnableBody>();
        body.prefabHandle = handle;
        return body;
    }

    public static void Despawn(SpawnableBody body)
    {
        //Make sure the object actually gets destroyed
        Assert.IsTrue(Addressables.ReleaseInstance(body.prefabHandle), "Failed to despawn spawnable body");
    }

    public AssetReferenceGameObject spawnedObject;

    public override async Task<SpawnableBody> Spawn()
    {
        return await SpawnAsync(spawnedObject, transform.position, transform.rotation);
    }

    private async void Start()
    {
        await Spawn();
    }

}