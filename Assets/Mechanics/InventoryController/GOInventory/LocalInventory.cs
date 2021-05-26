using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
	public class LocalInventory : GameObjectInventory
	{
		public ItemDatabase database;

		[MyBox.InitializationField] public MeleeWeaponItemData[] startingMeleeWeapons;

		private void Awake()
		{
			inv = ScriptableObject.CreateInstance<InventoryController>();
			inv.db = database;
			foreach (var m in startingMeleeWeapons)
			{
				inv.melee.AddItem(m, 1);
			}
		}
		[MyBox.ReadOnly] public InventoryController inv;

		public override InventoryController inventory => inv;
	}
}
