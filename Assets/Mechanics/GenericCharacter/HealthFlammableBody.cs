using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFlammableBody : FlammableBody
{
	public float damagePerSecond;
	public float damageInterval = 1;
	Health health;
	float nextDamageTick;

	protected override void Start()
	{
		health = GetComponent<Health>();

		base.Start();
		onFireLit.AddListener(OnFireLit);
	}
	void OnFireLit(bool lit)
	{
		nextDamageTick = Time.time + nextDamageTick;
	}

	private void Update()
	{
		if (health != null && onFire)
		{
			if (Time.time > nextDamageTick)
			{
				health.Damage(damagePerSecond * damageInterval, gameObject);
				nextDamageTick += damageInterval;
			}
		}
	}
}
