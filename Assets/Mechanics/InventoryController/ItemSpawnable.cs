using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ItemSpawnable : SpawnableBody
{
    [System.NonSerialized] public ItemName item;
    [System.NonSerialized] public uint count;

    public void Init(ItemName item, uint count)
    {
        this.item = item;
        this.count = count;
    }

    public void AddItemsToInventory()
    {
        InventoryController.AddItem(item, count, false);
    }

    public async void SpawnItemsToWorld()
    {
        Task<ItemSpawnable>[] t = new Task<ItemSpawnable>[count];

        for (int i = 0; i < count; i++)
        {
            t[i] = ItemSpawner.SpawnItemAsync(item, transform.position, transform.rotation);
        }

        await Task.WhenAll(t);
    }

}