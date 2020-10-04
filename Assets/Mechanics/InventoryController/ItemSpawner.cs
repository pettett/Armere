using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public static class Items
{

    public static Queue<GameObject> itemPhysicsPool = new Queue<GameObject>();
    public static Queue<GameObject> itemStaticPool = new Queue<GameObject>();


    public static async Task<InteractableItem> SpawnItem(PhysicsItemData physicsItem, Vector3 position, Quaternion rotation)
    {
        if (physicsItem == null)
        {
            Debug.LogError("No item to spawn");
            return null;
        }

        AsyncOperationHandle<GameObject> asyncLoad = physicsItem.spawnedGameobject.InstantiateAsync(position, rotation);
        await asyncLoad.Task;
        GameObject go = asyncLoad.Result;
        var interactable = go.AddComponent<InteractableItem>();
        var sphere = go.AddComponent<SphereCollider>();

        if (!physicsItem.staticPickup)
        {
            var meshCollider = go.AddComponent<MeshCollider>();
            var rb = go.AddComponent<Rigidbody>();

            //Set up the collider for this mesh
            meshCollider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.convex = true;
            rb.velocity = Vector3.zero;
        }

        sphere.radius = 2;
        sphere.isTrigger = true;

        interactable.Init(ItemSpawner.SpawnType.Item, physicsItem.itemName, 1);
        return interactable;
    }

    public static void DeSpawnItem(InteractableItem item)
    {
        if (item.type == ItemSpawner.SpawnType.Item)
        {
            Addressables.ReleaseInstance(item.gameObject);
        }
        else
        {
            MonoBehaviour.Destroy(item.gameObject);
        }
    }

    public static InteractableItem SpawnChest(GameObject prefab, ItemName item, uint count, Vector3 position, Quaternion rotation, ItemDatabase db)
    {
        var go = MonoBehaviour.Instantiate(prefab, position, rotation);
        var i = go.GetComponent<InteractableItem>();
        i.Init(ItemSpawner.SpawnType.Chest, item, count);
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
    [MyBox.ButtonMethod]
    async void SpawnItem()
    {
        if (spawnType == SpawnType.Item)
        {
            if (!(database[item] is PhysicsItemData))
            {
                Debug.LogError($"Cannot spawn {item}. Try using chest instead", gameObject);
                return;
            }

            var c = await Items.SpawnItem(database[item] as PhysicsItemData, transform.position, transform.rotation);

            c.onItemDestroy = DestroyItem;
        }
        else
        {
            var c = Items.SpawnChest(prefab, item, chestItemCount, transform.position, transform.rotation, database);
            c.onItemDestroy = DestroyItem;
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