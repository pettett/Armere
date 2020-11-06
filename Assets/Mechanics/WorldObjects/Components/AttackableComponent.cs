using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public sealed class AttackableComponent : WorldObjectComponent<AttackableComponentSettings>, IAttackable
{
    public Vector3 offset;
    Vector3 IScanable.offset => offset;
    public bool oneShotHit;
    public float health;

    public AttackResult Attack(ItemName weapon, GameObject attacker, Vector3 hitPosition)
    {
        if (oneShotHit)
        {
            DestroyWithItems();
            return AttackResult.Killed;
        }
        else
        {
            health -= ((WeaponItemData)InventoryController.singleton.db[weapon]).damage;
            if (health < 0)
            {
                DestroyWithItems();
                return AttackResult.Killed | AttackResult.Damaged;
            }
            else
            {
                return AttackResult.Damaged;
            }
        }
    }

    async void DestroyWithItems()
    {
        //Destroy the world object only after the objects have appeared
        gameObject.SetActive(false);
        await worldObject.SpawnContentsToWorld();
        WorldObjectSpawner.DestroyWorldObject(worldObject);
    }

    private void OnEnable()
    {
        TypeGroup<IAttackable>.allObjects.Add(this);
    }
    private void OnDisable()
    {
        TypeGroup<IAttackable>.allObjects.Remove(this);
    }
}
