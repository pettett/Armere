using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(menuName = "Game/Items/Item Data", fileName = "New Item Data")]
[AllowItemTypes(ItemType.Common, ItemType.Quest, ItemType.Currency, ItemType.Potion)]
public class ItemData : ScriptableObject
{
    public ItemName itemName;
    public ItemType type;
    public AssetReferenceSprite displaySprite;
    public string displayName = "New Item";
    [TextArea]
    public string description = "This item has no description";
    public bool sellable = true;
    public uint sellValue = 25u;
}
