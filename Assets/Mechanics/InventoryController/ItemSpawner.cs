using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Armere.Inventory;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{

	//Needs a GUID so it's state can be saved
	[RequireComponent(typeof(GuidComponent)), ExecuteAlways]
	public class ItemSpawner : Spawner
	{


		public bool spawnedItem;
		public AssetReferenceT<PhysicsItemData> item;




		public static AsyncOperationHandle<GameObject> SpawnItem(PhysicsItemData item, Vector3 position, Quaternion rotation)
		{
			Assert.IsNotNull(item, "Item cannot be null");
			Assert.IsTrue(item.gameObject.RuntimeKeyIsValid(), $"No gameobject reference for {item}");

			//Debug.Log($"Spawning {item.name}");
			void OnItemLoaded(AsyncOperationHandle<GameObject> handle)
			{
				ItemSpawnable spawnable = handle.Result.GetComponent<ItemSpawnable>();
				spawnable.Init(item, 1);
			}

			var handle = GameObjectSpawner.Spawn(item.gameObject, position, rotation);
			GameObjectSpawner.OnDone(handle, OnItemLoaded);

			return handle;
		}

		[MyBox.ButtonMethod]
		public void Spawn()
		{
			spawnedItem = true;
			if (item.RuntimeKeyIsValid())
				ItemDatabase.LoadItemDataAsync(item, data => SpawnItem(data, transform.position, transform.rotation));
			else
				Debug.LogWarning("Attemping to spawn invalid item");
		}

		//Broadcast by spawned item saver
		private void OnAfterGameLoaded()
		{
			//Debug.Log($"Started, {SaveManager.gameLoadingCompleted}");
			//TODO: Make this better - spawned item will be set by save manager before start

			if (Application.isPlaying)
				if (!spawnedItem)
				{
					Spawn();
				}
		}



#if UNITY_EDITOR

		private void Update()
		{
			if (Application.isPlaying) return;
			DrawSpawnedItem(item.editorAsset.gameObject);
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying) return;
			if (item.editorAsset.gameObject.editorAsset.TryGetComponent<MeshFilter>(out MeshFilter mf))
			{
				Mesh m = mf.sharedMesh;
				Gizmos.color = new Color(0, 1, 0, 0.02f);
				Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, item.editorAsset.gameObject.editorAsset.transform.lossyScale);
				Gizmos.DrawCube(m.bounds.center, m.bounds.size);
				Gizmos.color = new Color(0, 1, 0, 0.2f);

				Gizmos.DrawWireCube(m.bounds.center, m.bounds.size);
			}

		}
#endif

	}
}