using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Items
{

    public static Queue<GameObject> itemPhysicsPool = new Queue<GameObject>();
    public static Queue<GameObject> itemStaticPool = new Queue<GameObject>();


    public static InteractableItem SpawnItem(ItemName item, Vector3 position, Quaternion rotation, ItemDatabase db)
    {
        GameObject go;
        //Spawn the item from the pool
        if (db[item].staticPickup)
        {
            go = Spawner.Spawn(ref itemStaticPool, item.ToString(),
                typeof(MeshRenderer),
                typeof(MeshFilter),
                typeof(InteractableItem),
                typeof(SphereCollider));
        }
        else
        {
            go = Spawner.Spawn(ref itemPhysicsPool, item.ToString(),
                typeof(MeshRenderer),
                typeof(MeshFilter),
                typeof(Rigidbody),
                typeof(MeshCollider),
                typeof(InteractableItem),
                typeof(SphereCollider));

            //Set up the collider for this mesh
            go.GetComponent<MeshCollider>().sharedMesh = db[item].mesh;
            go.GetComponent<MeshCollider>().convex = true;
        }

        go.GetComponent<MeshRenderer>().materials = db[item].materials;
        go.GetComponent<MeshFilter>().mesh = db[item].mesh;

        go.transform.SetPositionAndRotation(position, rotation);

        go.GetComponent<SphereCollider>().radius = 2;
        go.GetComponent<SphereCollider>().isTrigger = true;

        //go.GetComponent<InteractableItem>().type = item;
        //re-enable the item in case it came from a pool
        var i = go.GetComponent<InteractableItem>();
        i.enabled = true;
        i.Init(ItemSpawner.SpawnType.Item, item, 1, db);
        return i;
    }
    public static InteractableItem SpawnChest(GameObject prefab, ItemName item, uint count, Vector3 position, Quaternion rotation, ItemDatabase db)
    {
        var go = MonoBehaviour.Instantiate(prefab, position, rotation);
        var i = go.GetComponent<InteractableItem>();
        i.Init(ItemSpawner.SpawnType.Chest, item, count, db);
        return i;
    }
    public static void OnSceneChange()
    {
        Debug.Log("New Scene");
        itemPhysicsPool = new Queue<GameObject>();
        itemStaticPool = new Queue<GameObject>();
    }
}

public class ItemSpawner : PlayerRelativeObject
{
    public enum SpawnType
    {
        Item,
        Chest
    }
    public SpawnType spawnType;
    public ItemDatabase database;
    public ItemName item;
    [MyBox.ConditionalField("spawnType", false, SpawnType.Chest)] public uint chestItemCount;
    [MyBox.ConditionalField("spawnType", false, SpawnType.Chest)] public GameObject prefab;
    GameObject spawned;
    private void Start()
    {
        SpawnItem();
        AddToRegister();
    }
    public override void OnPlayerInRange()
    {
        SpawnItem();
        base.OnPlayerInRange();
    }

    void SpawnItem()
    {
        if (spawnType == SpawnType.Item)
        {
            var c = Items.SpawnItem(item, transform.position, transform.rotation, database);
            c.onItemDestroy = DestroyItem;
            spawned = c.gameObject;
        }
        else
        {
            var c = Items.SpawnChest(prefab, item, chestItemCount, transform.position, transform.rotation, database);
            c.onItemDestroy = DestroyItem;
            spawned = c.gameObject;
        }

    }
    public void DestroyItem(InteractableItem gameObject)
    {
        if (spawnType == SpawnType.Item)
        {
            Invoke("SpawnItem", 2f);
        }
    }

}