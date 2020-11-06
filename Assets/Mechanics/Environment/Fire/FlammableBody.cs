using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FlammableBody : MonoBehaviour
{
    [System.Flags]
    public enum FireSpreadMode { None = 0, Contact = 1, Trigger = 2 }

    public FireSpreadMode spreadMode;
    public bool startLit;

    public float damagePerSecond;
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent onFireLit;
    public bool onFire = false;

    Health health;

    private void Start()
    {
        if (startLit)
        {
            Light();
        }

        health = GetComponent<Health>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (onFire && spreadMode.HasFlag(FireSpreadMode.Contact) && other.gameObject.TryGetComponent<FlammableBody>(out FlammableBody flammableBody))
        {
            if (!flammableBody.onFire) flammableBody.Light();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (onFire && spreadMode.HasFlag(FireSpreadMode.Trigger) && other.gameObject.TryGetComponent<FlammableBody>(out FlammableBody flammableBody))
        {
            //If the other collider is a trigger, make sure trigger spread is enabled for that body
            if ((other.isTrigger && flammableBody.spreadMode.HasFlag(FireSpreadMode.Trigger) || !other.isTrigger) && !flammableBody.onFire)
            {
                flammableBody.Light();
            }
        }
    }

    private void Update()
    {
        if (health != null && onFire)
        {
            health.Damage(damagePerSecond * Time.deltaTime, gameObject);
        }
    }


    public void Light() => SetFire(true);
    public void Extinguish() => SetFire(false);
    public void SetFire(bool enabled)
    {
        onFire = enabled;
        onFireLit.Invoke(enabled);
    }

}
