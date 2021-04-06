using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
	public class DropItemsOnDestroy : MonoBehaviour
	{
		public PhysicsItemData spawnedItem;
		public Vector2Int itemCount = new Vector2Int(1, 3);
		public Bounds localSpawnRange;
		public void DropItems()
		{
			if (spawnedItem != null)
			{
				int spawns = Random.Range(itemCount.x, itemCount.y + 1);


				for (int i = 0; i < spawns; i++)
				{
					Vector3 pos = new Vector3(
						Mathf.Lerp(localSpawnRange.min.x, localSpawnRange.max.x, Random.value),
						Mathf.Lerp(localSpawnRange.min.y, localSpawnRange.max.y, Random.value),
						Mathf.Lerp(localSpawnRange.min.z, localSpawnRange.max.z, Random.value)
					);

					ItemSpawner.SpawnItem(
						spawnedItem,
						transform.TransformPoint(pos),
						Quaternion.Euler(0, Random.Range(0, 360), 0));
				}
			}
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
