using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WorldObjectSpawner : MonoBehaviour
{

    public static async Task<WorldObject> SpawnItemAsync(PhysicsItemData itemData, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        //Item should not have data attached as it is a pickup
        return await SpawnWorldObjectAsync(itemData.worldObjectData, position, rotation, default, parent);
    }

    public static async Task<WorldObject> SpawnWorldObjectAsync(AssetReferenceT<WorldObjectData> worldObjectReference, Vector3 position, Quaternion rotation, WorldObjectInstanceData data, Transform parent = null)
    {
        WorldObjectData worldObjectData = await Addressables.LoadAssetAsync<WorldObjectData>(worldObjectReference).Task;

        WorldObject w = await SpawnWorldObjectAsync(worldObjectData, position, rotation, data, parent);
        Addressables.Release(worldObjectData);
        return w;
    }

    public static async Task<WorldObject> SpawnWorldObjectAsync(WorldObjectData worldObjectData, Vector3 position, Quaternion rotation, WorldObjectInstanceData data, Transform parent = null)
    {
        WorldObject w = await SpawnWorldObjectAsync(worldObjectData, data, parent);
        w.transform.SetPositionAndRotation(position, rotation);
        return w;
    }

    public static async Task<WorldObject> SpawnWorldObjectAsync(WorldObjectData worldObjectData, WorldObjectInstanceData data, Transform parent = null)
    {
        //do not track the handle for some extra performance
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(worldObjectData.gameObject, parent: parent, trackHandle: false);
        GameObject go = await handle.Task;


        WorldObject w = go.AddComponent<WorldObject>();
        w.prefabHandle = handle;


        switch (worldObjectData.isItemPickup)
        {
            case WorldObjectData.ItemPickupType.Auto:
                AddWorldObjectAddon<PassiveInteractableWorldObjectAddon>(w);
                w.instanceData = new WorldObjectInstanceData(worldObjectData.isItem, 1);
                break;
            case WorldObjectData.ItemPickupType.Manual:
                AddWorldObjectAddon<InteractableComponent>(w);
                SphereCollider interact = go.AddComponent<SphereCollider>();
                interact.isTrigger = true;
                interact.radius = 2;
                w.instanceData = new WorldObjectInstanceData(worldObjectData.isItem, 1);
                break;

            case WorldObjectData.ItemPickupType.Chest:
                AddWorldObjectAddon<ChestComponent>(w);
                goto case WorldObjectData.ItemPickupType.None;

            case WorldObjectData.ItemPickupType.None:
                w.instanceData = data;
                break;
        }

        if (worldObjectData.canBeDestroyed)
        {
            var attackable = AddWorldObjectAddon<AttackableComponent>(w);
            attackable.oneShotHit = worldObjectData.oneShotHit;
            attackable.health = worldObjectData.health;
            attackable.offset = worldObjectData.attackableTargetOffset;
        }

        if (worldObjectData.addWeaponTrigger || worldObjectData.addPhysics)
        {
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.convex = true;

            if (worldObjectData.triggerCollider)
            {
                collider.isTrigger = true;
                if (worldObjectData.addWeaponTrigger)
                {
                    WeaponTrigger weaponTrigger = go.AddComponent<WeaponTrigger>();
                }
            }
            else if (worldObjectData.addPhysics)
            {
                Rigidbody rb = go.AddComponent<Rigidbody>();
            }
        }


        if (worldObjectData.overrideMaterials != null && worldObjectData.overrideMaterials.Length != 0)
        {
            go.GetComponent<MeshRenderer>().materials = worldObjectData.overrideMaterials;
        }

        //Add all the types
        for (int i = 0; i < worldObjectData.componentSettings.Count; i++)
        {
            //Add the corresponding world object component
            WorldObjectComponent addon = (WorldObjectComponent)go.AddComponent(WorldObjectDataManager.instance.settingsTypes[worldObjectData.componentSettings[i].GetType()].monoBehaviour);
            addon.worldObject = w;
            addon.SetSettings(worldObjectData.componentSettings[i]);
        }

        return w;
    }

    public static void DestroyWorldObject(WorldObject worldObject)
    {
        Addressables.ReleaseInstance(worldObject.prefabHandle);
    }

    public static T AddWorldObjectAddon<T>(WorldObject worldObject) where T : WorldObjectComponent
    {
        T t = worldObject.gameObject.AddComponent<T>();
        t.worldObject = worldObject;
        return t;
    }

    public AssetReferenceT<WorldObjectData> worldObjectData;
    public ItemName containsItem;
    public uint containsItemCount = 1;


    private async void Start()
    {
        await SpawnWorldObjectAsync(worldObjectData, transform.position, transform.rotation,
                                    new WorldObjectInstanceData(containsItem, containsItemCount));
    }

}
