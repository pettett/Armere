using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FlammableBody : MonoBehaviour, IWaterObject, IExplosionEffector
{
	[System.Flags]
	public enum FireSpreadMode { None = 0, Contact = 1, Trigger = 2 }

	public FireSpreadMode spreadMode;
	public bool startLit;

	public UnityEvent<bool> onFireLit;
	public UnityEvent<bool> onFireExtinguish;

	public UnityEvent onFireStart;
	public UnityEvent onFireEnd;

	public bool onFire = false;
	public bool waterExtinguishes = true;
	public bool explosionLights = true;

	public void OnExplosion(Vector3 source, float radius, float force)
	{
		if (explosionLights)
			Light();
	}

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



	[MyBox.ButtonMethod]
	public void Light() => SetFire(true);
	[MyBox.ButtonMethod]
	public void Extinguish() => SetFire(false);
	public void SetFire(bool enabled)
	{
		onFire = enabled;
		onFireLit.Invoke(enabled);
		onFireExtinguish.Invoke(!enabled);

		if (enabled)
		{
			onFireStart.Invoke();
		}
		else
		{
			onFireEnd.Invoke();
		}

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
