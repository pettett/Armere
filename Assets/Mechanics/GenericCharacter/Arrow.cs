﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)),
RequireComponent(typeof(Rigidbody))]
public class Arrow : Projectile
{


	AmmoItemData ammo;
	bool destroyOnHit = false;


	public void LaunchArrow(AmmoItemData ammo, Vector3 velocity)
	{

		transform.localScale = Vector3.one;
		//Calibrate components
		this.ammo = ammo;

		LaunchProjectile(velocity);

		destroyOnHit = !ammo.flags.HasFlag(AmmoFlags.DropItemOnMiss);

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