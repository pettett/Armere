using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Items
{
    public static Queue<GameObject> itemPool = new Queue<GameObject>();

    public static InteractableItem SpawnItem(ItemName item, Vector3 position, Quaternion rotation, ItemDatabase db)
    {
        //Spawn the item from the pool
        var go = Spawner.Spawn(ref itemPool, item.ToString(),
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(Rigidbody),
            typeof(MeshCollider),
            typeof(InteractableItem),
            typeof(SphereCollider));

        go.GetComponent<MeshFilter>().mesh = db[item].mesh;
        go.GetComponent<MeshCollider>().sharedMesh = db[item].mesh;
        go.GetComponent<MeshCollider>().convex = true;

        go.GetComponent<MeshRenderer>().materials = db[item].materials;

        go.transform.SetPositionAndRotation(position, rotation);

        go.GetComponent<SphereCollider>().radius = 2;
        go.GetComponent<SphereCollider>().isTrigger = true;

        go.GetComponent<InteractableItem>().type = item;
        //re-enable the item in case it came from a pool
        go.GetComponent<InteractableItem>().enabled = true;
        return go.GetComponent<InteractableItem>();
    }
}

public class ItemSpawner : MonoBehaviour
{
    public ItemDatabase database;
    public ItemName item;
    private void Start()
    {
        SpawnItem();
    }
    void SpawnItem()
    {
        Items.SpawnItem(item, transform.position, transform.rotation, database).onItemDestroy += DestroyItem;
    }
    public void DestroyItem(InteractableItem gameObject)
    {
        gameObject.enabled = false;
        Spawner.DeSpawn(gameObject.gameObject, ref Items.itemPool);
        Invoke("SpawnItem", 2f);
    }
}