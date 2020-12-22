using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Armere.Inventory
{

    [CreateAssetMenu(menuName = "Game/Items/Physics Item Data", fileName = "New Physics Item Data")]
    [AllowItemTypes(ItemType.Common, ItemType.Currency)]
    public class PhysicsItemData : ItemData
    {
        public AssetReferenceGameObject gameObject;
    }
}