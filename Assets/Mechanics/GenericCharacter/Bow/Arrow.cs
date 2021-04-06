using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)),
RequireComponent(typeof(Rigidbody))]
public class Arrow : Projectile
{


	AmmoItemData ammo;
	public TrailRenderer trailRenderer;
	bool destroyOnHit = false;

	private void Start()
	{
		trailRenderer.emitting = false;
	}
	public void LaunchArrow(AmmoItemData ammo, Vector3 velocity)
	{
		//Calibrate components
		this.ammo = ammo;

		LaunchProjectile(velocity);

		destroyOnHit = !ammo.flags.HasFlag(AmmoFlags.DropItemOnMiss);

		trailRenderer.emitting = true;

	}


	public override void OnProjectileHit(Collision collision, out bool goodHit)
	{
		base.OnProjectileHit(collision, out goodHit);

		if (!destroyOnHit && !goodHit)
		{
			//Turn arrow into an item if it is permitted
			ItemSpawner.SpawnItem(ammo, transform.position, transform.rotation);
		}
	}
}