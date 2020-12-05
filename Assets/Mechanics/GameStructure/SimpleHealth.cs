using UnityEngine;
using UnityEngine.Events;

public class SimpleHealth : MonoBehaviour
{
    //so you can see health in inspector
    [ReadOnly] public float currentHealth;

    public float health { get; protected set; }
    public float maxHealth;


    public UnityEvent onDeathEvent;
    protected CollisionAudio audioSet;
    protected virtual void Start()
    {
        health = maxHealth;
        audioSet = GetComponent<CollisionAudio>();
    }



    public virtual AttackResult Damage(float amount, GameObject source)
    {
        if (audioSet != null) audioSet.MakeNoise(transform.position, 20);

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