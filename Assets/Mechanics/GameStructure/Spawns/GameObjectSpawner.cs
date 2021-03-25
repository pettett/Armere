using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Assertions;
using System.Collections;

[ExecuteAlways]
public class GameObjectSpawner : Spawner
{
	static void OnOperationComplete(AsyncOperationHandle<GameObject> handle)
	{
		var body = handle.Result.GetComponent<SpawnableBody>();
		Assert.IsNotNull(body, $"{handle.Result.name} has no SpawnableBody component");
		body.prefabHandle = handle;
		body.Init();
	}



	public static AsyncOperationHandle<GameObject> Spawn(AssetReferenceGameObject gameObject, Transform parent = null, bool instantiateInWorldSpace = false)
	{
		AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(gameObject, parent, instantiateInWorldSpace, false);
		OnDone(handle, OnOperationComplete);
		return handle;
	}

	public static AsyncOperationHandle<GameObject> Spawn(AssetReferenceGameObject gameObject, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		var handle = Addressables.InstantiateAsync(gameObject, position, rotation, parent, false);

		OnDone(handle, OnOperationComplete);

		return handle;
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

	public void StartSpawn()
	{
		Spawn();
	}


	public void Spawn()
	{
		Assert.IsTrue(spawnedObject.RuntimeKeyIsValid(), "Reference is null");
		Spawn(spawnedObject, transform.position, transform.rotation);
	}

	private void Start()
	{
#if UNITY_EDITOR
		if (Application.isPlaying)
#endif
			Spawn();
	}

#if UNITY_EDITOR

	private void Update()
	{
		if (Application.isPlaying) return;
		DrawSpawnedItem(spawnedObject);
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying) return;
		if (spawnedObject.editorAsset.TryGetComponent<MeshFilter>(out MeshFilter mf))
		{
			Mesh m = mf.sharedMesh;
			Gizmos.color = new Color(0, 1, 0, 0.02f);
			Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, spawnedObject.editorAsset.transform.lossyScale);
			Gizmos.DrawCube(m.bounds.center, m.bounds.size);
			Gizmos.color = new Color(0, 1, 0, 0.2f);

			Gizmos.DrawWireCube(m.bounds.center, m.bounds.size);
		}
	}
#endif

}