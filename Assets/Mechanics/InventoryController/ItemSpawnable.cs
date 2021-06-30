using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Armere.Inventory;
using UnityEngine.Assertions;

namespace Armere.Inventory
{

	public class ItemSpawnable : SpawnableBody
	{
		[System.NonSerialized] public ItemData item;
		[System.NonSerialized] public uint count;

		public void Init(ItemData item, uint count)
		{
			Assert.IsNotNull(item, "Item cannot be null");
			this.item = item;
			this.count = count;
		}
		private void Start()
		{
			if (item == null)
				Debug.LogError($"Item {gameObject.name} has not been inited", this);
		}

		public void AddItemsToInventory(System.Action onItemAdded, InventoryController inventory)
		{
			if (item == null)
			{
				throw new System.ArgumentException("Item Cannot be null");
			}
			if (inventory.TryAddItem(item, count, false))
			{
				onItemAdded?.Invoke();
			}
			else
			{
				//Open ui to see if the player wants to replace an item
				inventory.ReplaceItemDialogue(item, onItemAdded);
			}
		}
	}
}