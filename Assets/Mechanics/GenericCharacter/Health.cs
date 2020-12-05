using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : SimpleHealth, IAttackable
{


    public float headshotHeight = 1.5f;
    public bool useUI;
    public string uiName = "health";
    public bool dead { get; private set; }

    public Vector3 offset => centerOffset;

    public Vector3 centerOffset;

    public delegate void HealthEvent(GameObject attacker, GameObject victim);

    public delegate void ShieldDamage(ref float damage, GameObject attacker, GameObject victim);

    public event HealthEvent onDeath;
    public bool blockingDamage = false;
    [Range(-1, 1)]
    public float minBlockingDot = 0.5f;

    public event HealthEvent onTakeDamage;
    public event HealthEvent onBlockDamage;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        currentHealth = health;
    }
    public void Respawn()
    {
        health = maxHealth;
        currentHealth = health;
        dead = false;
    }

    public override AttackResult Damage(float amount, GameObject origin)
    {

        if (dead)
            return AttackResult.None;


        if (audioSet != null) audioSet.MakeNoise(transform.position, 20);

        //Test if the damage can be blocked based on the angle
        //TODO: Better blocking physics
        else if (blockingDamage && Vector3.Dot(transform.forward, (origin.transform.position - transform.position).normalized) > minBlockingDot)
        {
            onBlockDamage?.Invoke(origin, gameObject);
            return AttackResult.Blocked;
        }

        AttackResult r = AttackResult.Damaged;

        SetHealth(health - amount);

        if (health == 0)
        {
            dead = true;
            onDeath?.Invoke(origin, gameObject);
            onDeathEvent.Invoke();

            r |= AttackResult.Killed;
        }
        else
        {
            onTakeDamage?.Invoke(origin, gameObject);
        }
        UpdateUI();

        return r;
    }

    public void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        currentHealth = health;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position + Vector3.up * headshotHeight, new Vector3(1, 0, 1));

        Gizmos.DrawWireSphere(transform.position + offset, 0.1f);
    }

    void UpdateUI()
    {
        if (useUI)
            ProgressionBar.SetInstanceProgress(uiName, health, maxHealth);
    }

    public AttackResult Attack(ItemName weapon, GameObject controller, Vector3 hitPosition)
    {
        WeaponItemData weaponData = (WeaponItemData)InventoryController.singleton.db[weapon];
        return Damage(weaponData.damage, controller);
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
