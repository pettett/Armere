using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MyBox;
using System;

[Serializable]
public abstract class WorldObjectDataComponentSettings : ScriptableObject
{

}



[CreateAssetMenu(fileName = "World Object Data", menuName = "Game/World Object Data", order = 0)]
public class WorldObjectData : ScriptableObject
{
    [NonSerialized] public bool isDirty = false;


    public AssetReferenceGameObject gameObject;

    public bool containsItems = false;

    public enum ItemPickupType { None, Auto, Manual, Chest }
    public ItemPickupType isItemPickup = ItemPickupType.None;
    [ConditionalField("isItemPickup", false, ItemPickupType.Auto, ItemPickupType.Manual)] public ItemName isItem;

    public bool canBeDestroyed;
    [ConditionalField("canBeDestroyed")] public bool oneShotHit;
    [ConditionalField("canBeDestroyed")] public Vector3 attackableTargetOffset;
    [ConditionalField("oneShotHit", true)] public float health;

    public bool canBeBurnt;
    public bool addPhysics;
    public bool triggerCollider;
    public bool addWeaponTrigger;

    public List<WorldObjectDataComponentSettings> componentSettings = new List<WorldObjectDataComponentSettings>();

    public bool HasSettings(Type type)
    {
        foreach (var setting in componentSettings)
        {
            if (setting.GetType() == type)
                return true;
        }
        return false;
    }


    public Material[] overrideMaterials;


    // [SerializeReference] public WorldObjectDataComponentBase[] components;

    private void OnValidate()
    {
        if (addWeaponTrigger)
        {
            addPhysics = false;
            triggerCollider = true;
        }
    }
}