using UnityEngine;
using UnityEngine.Events;

public class SimpleHealth : MonoBehaviour
{
    //so you can see health in inspector
    [ReadOnly] public float currentHealth;

    public float health { get; protected set; }
    public float maxHealth;


    public UnityEvent onDeathEvent;
    private void Start()
    {
        health = maxHealth;
    }
    public virtual AttackResult Damage(float amount, GameObject source)
    {
        health -= amount;

        currentHealth = health;

        if (health < 0)
        {
            health = 0;
            return AttackResult.Damaged | AttackResult.Killed;
        }
        return AttackResult.Damaged;
    }


}