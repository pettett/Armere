using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Armere.Inventory
{
	public abstract class GameObjectInventory : MonoBehaviour
	{
		public abstract InventoryController inventory { get; }

		public bool HasMeleeWeapon => inventory.melee.items.Count > 0;

		public int BestMeleeWeapon
		{
			get
			{
				int best = -1;
				for (int i = 0; i < inventory.melee.items.Count; i++)
				{
					if (best == -1 || ((MeleeWeaponItemData)inventory.melee.items[best].item).damage < ((MeleeWeaponItemData)inventory.melee.items[i].item).damage)
					{
						best = i;
					}
				}

				return best;
			}
		}


		public MeleeWeaponItemData SelectedMeleeWeapon => (MeleeWeaponItemData)inventory.melee.items[inventory.selectedMelee].item;


	}
}
