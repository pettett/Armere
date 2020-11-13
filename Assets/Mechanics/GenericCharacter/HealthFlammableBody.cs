using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFlammableBody : FlammableBody
{
    public float damagePerSecond;
    Health health;
    protected override void Start()
    {
        health = GetComponent<Health>();

        base.Start();
    }

    private void Update()
    {
        if (health != null && onFire)
        {
            health.Damage(damagePerSecond * Time.deltaTime, gameObject);
        }
    }
}
