using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Armere.Inventory;
namespace Armere.Inventory
{

	public class ItemSpawnable : SpawnableBody
	{
		public ItemData item;
		public uint count;

		public void Init(ItemData item, uint count)
		{
			this.item = item;
			this.count = count;
		}

		public void AddItemsToInventory()
		{
			if (item == null)
			{
				throw new System.ArgumentException("Item Cannot be null");
			}
			InventoryController.singleton.AddItem(item, count, false);
		}

		// public async void SpawnItemsToWorld()
		// {
		// 	Task<ItemSpawnable>[] t = new Task<ItemSpawnable>[count];

		// 	for (int i = 0; i < count; i++)
		// 	{
		// 		t[i] = ItemSpawner.SpawnItemAsync(()item, transform.position, transform.rotation);
		// 	}

		// 	await Task.WhenAll(t);
		// }

	}
}