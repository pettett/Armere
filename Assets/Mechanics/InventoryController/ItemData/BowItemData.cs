using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{

	[CreateAssetMenu(menuName = "Game/Items/Bow Item Data", fileName = "New Bow Item Data")]
	[AllowItemTypes(ItemType.Bow)]
	public class BowItemData : WeaponItemData
	{
	}
}
