
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Ammo Item Data", fileName = "New Ammo Item Data")]
[AllowItemTypes(ItemType.Ammo)]
public class AmmoItemData : PhysicsItemData
{
    public AssetReferenceGameObject ammoGameObject;
}