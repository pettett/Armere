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
		body.Init();
		return body;
	}

	public static async Task<SpawnableBody> SpawnAsync(AssetReferenceGameObject gameObject, Transform parent = null, bool instantiateInWorldSpace = false)
	{
		var handle = Addressables.InstantiateAsync(gameObject, parent, instantiateInWorldSpace, false);
		var body = (await handle.Task).GetComponent<SpawnableBody>();
		body.prefabHandle = handle;
		body.Init();
		return body;
	}

	public static void Despawn(SpawnableBody body)
	{
		//Make sure the object actually gets destroyed
		if (body.prefabHandle.IsValid())
			Addressables.ReleaseInstance(body.prefabHandle);
		else
			Destroy(body.gameObject);
	}

	public AssetReferenceGameObject spawnedObject;

	public override async Task<SpawnableBody> Spawn()
	{
		Assert.IsTrue(spawnedObject.RuntimeKeyIsValid(), "Reference is null");
		return await SpawnAsync(spawnedObject, transform.position, transform.rotation);
	}

	private async void Start()
	{
		await Spawn();
	}
#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (Application.isPlaying) return;
		Mesh m = spawnedObject.editorAsset.GetComponent<MeshFilter>().sharedMesh;
		var mats = spawnedObject.editorAsset.GetComponent<MeshRenderer>().sharedMaterials;

		for (int i = 0; i < m.subMeshCount; i++)
		{
			//Graphics.DrawMesh(m, transform.position, transform.rotation, mats[i], 0);
			Gizmos.color = mats[i].color;
			Gizmos.DrawMesh(m, i, transform.position, transform.rotation, transform.lossyScale);
		}

	}
#endif

}