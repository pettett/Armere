using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public Dictionary<ItemName, int> items;
    public static InventoryController singleton;
    private void Awake()
    {
        items = new Dictionary<ItemName, int>();
        singleton = this;

    }
    public static int GetItemCount(ItemName item)
    {
        if (!singleton.items.ContainsKey(item))
            return 0;
        else
            return singleton.items[item];
    }
    public static void AddItem(ItemName item)
    {
        if (!singleton.items.ContainsKey(item))
            singleton.items[item] = 1;
        else
            singleton.items[item]++;
    }
    public static void TakeItems(ItemName item, int count)
    {
        if (singleton.items.ContainsKey(item))
            singleton.items[item] -= count;
    }
}
