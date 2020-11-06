using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Physics Item Data", fileName = "New Physics Item Data")]
[AllowItemTypes(ItemType.Common, ItemType.Currency)]
public class PhysicsItemData : ItemData
{

    public AssetReferenceT<WorldObjectData> worldObjectData;
    public AssetReferenceGameObject gameObject;
}

