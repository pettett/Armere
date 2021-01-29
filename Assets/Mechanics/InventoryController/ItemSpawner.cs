using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using Armere.Inventory;
namespace Armere.Inventory
{

	//Needs a GUID so it's state can be saved
	[RequireComponent(typeof(GuidComponent))]
	public class ItemSpawner : Spawner
	{


		public bool spawnedItem;
		public PhysicsItemData item;

		public static async Task<ItemSpawnable> SpawnItemAsync(PhysicsItemData item, Vector3 position, Quaternion rotation)
		{
			Assert.IsTrue(item.gameObject.RuntimeKeyIsValid(), $"No gameobject reference for {item}");

			ItemSpawnable spawnable = (ItemSpawnable)await GameObjectSpawner.SpawnAsync(item.gameObject, position, rotation);
			spawnable.Init(item, 1);
			return spawnable;
		}

		public override async Task<SpawnableBody> Spawn()
		{
			spawnedItem = true;
			return await SpawnItemAsync(item, transform.position, transform.rotation);
		}

		//Broadcast by spawned item saver
		private void OnAfterGameLoaded()
		{
			//Debug.Log($"Started, {SaveManager.gameLoadingCompleted}");
			//TODO: Make this better - spawned item will be set by save manager before start
			if (!spawnedItem)
			{
				var x = Spawn();
			}
		}
	}
}