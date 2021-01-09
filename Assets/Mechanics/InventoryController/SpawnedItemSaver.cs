using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[System.Serializable]
public readonly struct SpawnedItemRecord
{
    public readonly Vector3 position;
    public readonly Quaternion rotation;
    public readonly ItemName itemName;
    public readonly uint bundleSize;

    public SpawnedItemRecord(ItemSpawnable item)
    {
        this.position = item.transform.position;
        this.rotation = item.transform.rotation;
        this.itemName = item.item;
        this.bundleSize = item.count;
    }
}

public class SpawnedItemSaver : MonoSaveable<SpawnedItemRecord[]>
{
    public override void LoadBlank()
    {
        //Do nothing
    }

    public override void LoadData(SpawnedItemRecord[] spawnedItemData)
    {
        Profiler.BeginSample("Restoring Item Spawnable Objects");
        //Replace all the spawnable item objects that were in the scene before hand

        for (int i = 0; i < spawnedItemData.Length; i++)
        {
            var x = ItemSpawner.SpawnItemAsync(spawnedItemData[i].itemName, spawnedItemData[i].position, spawnedItemData[i].rotation);
        }
        Profiler.EndSample();

    }

    public override SpawnedItemRecord[] SaveData()
    {
        var spawnedItems = MonoBehaviour.FindObjectsOfType<ItemSpawnable>();
        SpawnedItemRecord[] records = new SpawnedItemRecord[spawnedItems.Length];
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            records[i] = new SpawnedItemRecord(spawnedItems[i]);
        }
        return records;
    }
}
