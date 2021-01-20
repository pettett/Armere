
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Armere.Inventory
{

	[System.Flags]
	public enum AmmoFlags
	{
		None = 0,
		DropItemOnMiss = 1, // Drop an item if the ammo hits nothing
		DropItemOnHit = 2, //Drop an item if the ammo hits an enemy
	}

	[CreateAssetMenu(menuName = "Game/Items/Ammo Item Data", fileName = "New Ammo Item Data")]
	[AllowItemTypes(ItemType.Ammo)]
	public class AmmoItemData : PhysicsItemData
	{
		[Header("Ammo")]
		public AssetReferenceGameObject ammoGameObject;
		public AmmoFlags flags;
	}
}