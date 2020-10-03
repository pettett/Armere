using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour, IAttackable
{
    float _health;
    //so you can see health in inspector
    [ReadOnly] public float health;

    public float maxHealth;
    public float headshotHeight = 1.5f;

    public bool useUI;
    public string uiName = "health";


    public bool dead { get; private set; }


    public delegate void HealthEvent(GameObject attacker, GameObject victim);

    public delegate void ShieldDamage(ref float damage, GameObject attacker, GameObject victim);

    public event HealthEvent onDeath;
    public event ShieldDamage TryShieldDamage;
    public UnityEvent onDeathEvent;

    public event HealthEvent onTakeDamage;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _health = maxHealth;
        health = _health;
    }
    public void Respawn()
    {
        _health = maxHealth;
        health = _health;
        dead = false;
    }
    public void Damage(float amount, GameObject origin)
    {

        if (dead)
            return;

        TryShieldDamage?.Invoke(ref amount, origin, gameObject);

        _health -= amount;
        _health = Mathf.Clamp(_health, 0, maxHealth);
        health = _health;
        if (_health == 0)
        {
            dead = true;
            onDeath?.Invoke(origin, gameObject);
            onDeathEvent.Invoke();
        }
        else
        {
            onTakeDamage?.Invoke(origin, gameObject);
        }
        UpdateUI();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position + Vector3.up * headshotHeight, new Vector3(1, 0, 1));
    }


    void UpdateUI()
    {
        if (useUI)
            ProgressionBar.SetInstanceProgress(uiName, _health, maxHealth);
    }

    public void Attack(ItemName weapon, GameObject controller, Vector3 hitPosition)
    {
        WeaponItemData weaponData = (WeaponItemData)InventoryController.singleton.db[weapon];
        print(weaponData.damage);
        Damage(weaponData.damage, controller);
    }
}
