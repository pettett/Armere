using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
	public class AssetInventory : GameObjectInventory
	{
		public InventoryController inv;

		public override InventoryController inventory => inv;
	}
}
