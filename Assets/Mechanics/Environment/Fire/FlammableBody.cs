using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FlammableBody : MonoBehaviour, IWaterObject
{
    [System.Flags]
    public enum FireSpreadMode { None = 0, Contact = 1, Trigger = 2 }

    public FireSpreadMode spreadMode;
    public bool startLit;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent onFireLit;
    public bool onFire = false;
    public bool waterExtinguishes = true;

    protected virtual void Start()
    {
        if (startLit)
        {
            Light();
        }
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




    public void Light() => SetFire(true);
    public void Extinguish() => SetFire(false);
    public void SetFire(bool enabled)
    {
        onFire = enabled;
        onFireLit.Invoke(enabled);
    }

    public void OnWaterEnter(WaterController waterController)
    {
        if (onFire && waterExtinguishes) Extinguish();
    }

    public void OnWaterExit(WaterController waterController)
    {
        //Doesnt matter
    }
}
