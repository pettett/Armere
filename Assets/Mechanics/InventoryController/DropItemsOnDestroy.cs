using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{
	public class DropItemsOnDestroy : MonoBehaviour
	{
		public AssetReferenceT<PhysicsItemData> spawnedItem;
		PhysicsItemData data;
		public Vector2Int itemCount = new Vector2Int(1, 3);
		[System.NonSerialized] public Bounds localSpawnRange;
		public void DropItems()
		{
			if (data != null)
			{

				int spawns = Random.Range(itemCount.x, itemCount.y + 1);
				for (int i = 0; i < spawns; i++)
				{
					Vector3 pos = transform.TransformPoint(new Vector3(
						Mathf.Lerp(localSpawnRange.min.x, localSpawnRange.max.x, Random.value),
						Mathf.Lerp(localSpawnRange.min.y, localSpawnRange.max.y, Random.value),
						Mathf.Lerp(localSpawnRange.min.z, localSpawnRange.max.z, Random.value)
					));

					ItemSpawner.SpawnItem(data, pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
				}

			}
		}
		private void Start()
		{
			localSpawnRange = GetComponent<Collider>().bounds;

			ItemDatabase.LoadItemDataAsync(spawnedItem, item =>
			{
				data = item;
			});
		}

		bool isQuitting = false;
		void OnApplicationQuit()
		{
			isQuitting = true;
		}

		private void OnDestroy()
		{
			if (!isQuitting)
				DropItems();
		}
		private void OnDrawGizmos()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawWireCube(localSpawnRange.center, localSpawnRange.size);
		}
	}
}
