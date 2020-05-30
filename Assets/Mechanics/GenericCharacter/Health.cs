using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour
{
    float _health;
    //so you can see health in inspector
    [ReadOnly] public float health;

    public float maxHealth;
    public float headshotHeight = 1.5f;

    public bool useUI;
    public string uiName = "health";


    public bool dead { get; private set; }


    public delegate void EventDelegate(GameObject attacker, GameObject victim);

    public event EventDelegate onDeath;
    public UnityEvent onDeathEvent;

    public event EventDelegate onTakeDamage;

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
        //if (useUI)
        //ProgressionBar.SetInstanceProgress(uiName, _health, maxHealth);
    }

}
