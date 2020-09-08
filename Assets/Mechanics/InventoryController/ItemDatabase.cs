using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase", order = 0)]
public class ItemDatabase : ScriptableObject
{
    public ItemData this[ItemName key] => itemData[(int)key];

    public ItemData[] itemData;
}