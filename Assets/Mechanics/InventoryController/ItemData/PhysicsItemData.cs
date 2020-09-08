using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Physics Item Data", fileName = "New Physics Item Data")]
[AllowItemTypes(ItemType.Common)]
public class PhysicsItemData : ItemData
{
    public bool staticPickup;
    public AssetReferenceGameObject spawnedGameobject;
}
