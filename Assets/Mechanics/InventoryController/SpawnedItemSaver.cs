using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


namespace Armere.Inventory
{

	public class SpawnedItemSaver : MonoSaveable
	{
		public override void LoadBlank()
		{
			//Do nothing
		}

		//Save format:
		/*
		int COUNT:
		foreach in COUNT
			-Vec3 position
			-quat rotation
			-int itemName
			-uint bundleSize
		*/

		public override void SaveBin(GameDataWriter writer)
		{
			Profiler.BeginSample("Saving Item Spawnable Objects");

			var spawnedItems = MonoBehaviour.FindObjectsOfType<ItemSpawnable>();
			writer.Write(spawnedItems.Length);
			for (int i = 0; i < spawnedItems.Length; i++)
			{
				writer.Write(spawnedItems[i].transform.position);
				writer.Write(spawnedItems[i].transform.rotation);
				writer.Write((int)spawnedItems[i].item);
				writer.Write(spawnedItems[i].count);
			}


			Profiler.EndSample();
			Profiler.BeginSample("Saving Item Spawners");

			var spawners = MonoBehaviour.FindObjectsOfType<ItemSpawner>();
			writer.Write(spawners.Length);
			for (int i = 0; i < spawners.Length; i++)
			{
				writer.Write(spawners[i].GetComponent<GuidComponent>().GetGuid());
				writer.Write(spawners[i].spawnedItem);
			}

			Profiler.EndSample();
		}
		public override void LoadBin(Version saveVersion, GameDataReader reader)
		{
			if (saveVersion != SaveManager.version)
			{
				Debug.Log("Incorrect version");
			}
			Profiler.BeginSample("Restoring Item Spawnable Objects");

			int numItems = reader.ReadInt();
			for (int i = 0; i < numItems; i++)
			{
				Vector3 pos = reader.ReadVector3();
				Quaternion rot = reader.ReadQuaternion();
				ItemName item = (ItemName)reader.ReadInt();
				uint bundleSize = reader.ReadUInt();

				var x = ItemSpawner.SpawnItemAsync(item, pos, rot);

			}

			Profiler.EndSample();
			Profiler.BeginSample("Loading Item Spawners");

			int numSpawners = reader.ReadInt();


			var itemSpawnerData = new Dictionary<System.Guid, bool>();

			for (int i = 0; i < numSpawners; i++)
			{
				itemSpawnerData[reader.ReadGuid()] = reader.ReadBool();
			}

			foreach (var spawner in MonoBehaviour.FindObjectsOfType<ItemSpawner>())
			{
				if (itemSpawnerData.TryGetValue(spawner.GetComponent<GuidComponent>().GetGuid(), out var data))
				{
					//This should be set before the spawner's start value
					spawner.spawnedItem = data;
				}
			}

			Profiler.EndSample();
		}
	}
}