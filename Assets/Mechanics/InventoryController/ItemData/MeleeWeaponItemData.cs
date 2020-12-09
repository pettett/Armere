using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Melee Weapon Item Data", fileName = "New Melee Weapon Item Data")]
[AllowItemTypes(ItemType.Melee)]
public class MeleeWeaponItemData : WeaponItemData
{
    public AttackFlags attackFlags;
    public AssetReferenceGameObject hitSparkEffect;
}
