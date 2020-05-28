using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public ItemDatabase db;
    public Dictionary<ItemType, Dictionary<ItemName, int>> items = new Dictionary<ItemType, Dictionary<ItemName, int>>(){
        {ItemType.Common,new Dictionary<ItemName, int>()},
        {ItemType.Quest,new Dictionary<ItemName, int>()},
        {ItemType.Weapon,new Dictionary<ItemName, int>()},
    };

    public static InventoryController singleton;
    private void Awake()
    {
        items = new Dictionary<ItemType, Dictionary<ItemName, int>>(){
            {ItemType.Common,new Dictionary<ItemName, int>()},
            {ItemType.Quest,new Dictionary<ItemName, int>()},
            {ItemType.Weapon,new Dictionary<ItemName, int>()},
        };

        singleton = this;
    }
    static Dictionary<ItemName, int> ItemTypeDict(ItemName target) => singleton.items[singleton.db[target].type];
    static int ItemCount(ItemName target) => ItemTypeDict(target)[target];
    public static int GetItemCount(ItemName item)
    {
        if (!ItemTypeDict(item).ContainsKey(item))
            return 0;
        else
            return ItemCount(item);
    }
    public static void AddItem(ItemName item)
    {
        if (!ItemTypeDict(item).ContainsKey(item))
            ItemTypeDict(item)[item] = 1;
        else
            ItemTypeDict(item)[item]++;
    }
    public static void TakeItems(ItemName item, int count)
    {
        if (ItemTypeDict(item).ContainsKey(item))
            ItemTypeDict(item)[item] -= count;
    }
}
