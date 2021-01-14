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
		public ItemName item;

		public static async Task<ItemSpawnable> SpawnItemAsync(ItemName item, Vector3 position, Quaternion rotation)
		{
			var go = ((PhysicsItemData)InventoryController.singleton.db[item]).gameObject;

			Assert.IsTrue(go.RuntimeKeyIsValid(), $"No gameobject reference for {item}");

			ItemSpawnable spawnable = (ItemSpawnable)await GameObjectSpawner.SpawnAsync(go, position, rotation);
			spawnable.Init(item, 1);
			return spawnable;
		}

		public override async Task<SpawnableBody> Spawn()
		{
			spawnedItem = true;
			return await SpawnItemAsync(item, transform.position, transform.rotation);
		}

		private void Start()
		{
			Debug.Log($"Started, {SaveManager.gameLoadingCompleted}");
			//TODO: Make this better - spawned item will be set by save manager before start
			if (!spawnedItem)
			{
				var x = Spawn();
			}
		}
	}
}