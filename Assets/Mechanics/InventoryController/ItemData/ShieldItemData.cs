using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Shield Item Data", fileName = "New Shield Item Data")]
[AllowItemTypes(ItemType.SideArm)]
public class ShieldItemData : SideArmItemData
{

    [Range(-1, 1)]
    public float minBlockingDot = 0.2f;
}
