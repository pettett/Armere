using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;




namespace Armere.Inventory
{

	[CreateAssetMenu(menuName = "Game/Items/Melee Weapon Item Data", fileName = "New Melee Weapon Item Data")]
	[AllowItemTypes(ItemType.Melee)]
	public class MeleeWeaponItemData : WeaponItemData
	{
		[Header("Melee weapon")]
		public AttackFlags attackFlags;

		public bool isDoubleHanded;
		public AssetReferenceGameObject hitSparkEffect;
	}
}