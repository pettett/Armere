﻿using System.Collections;
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

		public void AddItemsToInventory(System.Action onItemAdded)
		{
			if (item == null)
			{
				throw new System.ArgumentException("Item Cannot be null");
			}
			if (InventoryController.singleton.TryAddItem(item, count, false))
			{
				onItemAdded?.Invoke();
			}
			else
			{
				//Open ui to see if the player wants to replace an item
				InventoryController.singleton.ReplaceItemDialogue(item, onItemAdded);
			}
		}

	}
}