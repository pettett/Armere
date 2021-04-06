using UnityEngine;
using UnityEngine.Events;

public class SimpleHealth : MonoBehaviour
{
	//so you can see health in inspector

	public float health = 100;
	public float maxHealth = 100;
	public float startingHealth = 100;

	public UnityEvent onDeathEvent;
	protected CollisionAudio audioSet;
	protected virtual void Start()
	{
		health = Mathf.Clamp(startingHealth, 0, maxHealth);
		if (health == 0)
		{
			health = maxHealth;
		}


		audioSet = GetComponent<CollisionAudio>();
	}



	public virtual AttackResult Damage(float amount, GameObject source)
	{
		if (audioSet != null) audioSet.MakeNoise(transform.position, 20);

		health -= amount;

		if (health < 0)
		{
			Die();
			return AttackResult.Damaged | AttackResult.Killed;
		}
		return AttackResult.Damaged;
	}
	public virtual void Die()
	{
		health = 0;
	}

}