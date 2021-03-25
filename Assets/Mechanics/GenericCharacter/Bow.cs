﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
using UnityEngine.Assertions;

public class Bow : SpawnableBody
{
	public Transform arrowAnchor;
	public Transform backHandAnchor;
	public Transform arrowSpawnPosition;
	Arrow notchedArrow;
	AmmoItemData currentAmmo;
	public BowItemData bowItem;

	public void InitBow(BowItemData bowItem)
	{
		this.bowItem = bowItem;
	}
	public async void NotchNextArrow(AmmoItemData ammo)
	{
		Assert.IsNotNull(bowItem, "Bow not innitted");

		this.currentAmmo = ammo;
		//If an arrow is already notched, remove it and replaced
		if (notchedArrow != null) RemoveNotchedArrow();

		//create the arrow that should be fired next

		notchedArrow = (await GameObjectSpawner.Spawn(ammo.ammoGameObject, arrowAnchor).Task).GetComponent<Arrow>();

		Assert.IsNotNull(notchedArrow, "No arrow on ammo");

		notchedArrow.InitProjectile(bowItem.damage);
		notchedArrow.transform.localScale = Vector3.one * 0.01f;
		notchedArrow.transform.localRotation = Quaternion.Euler(90, 0, 0);
	}

	public void RemoveNotchedArrow()
	{
		notchedArrow?.Destroy();
		notchedArrow = null;
	}

	private void OnDestroy()
	{
		RemoveNotchedArrow();
	}

	public void ReleaseArrow(Vector3 velocity)
	{
		UnityEngine.Assertions.Assert.IsNotNull(notchedArrow);


		notchedArrow.transform.SetParent(null);

		notchedArrow.transform.localScale = Vector3.one;

		notchedArrow.transform.position = arrowSpawnPosition.position + arrowSpawnPosition.forward * 1;
		notchedArrow.LaunchArrow(currentAmmo, velocity);

		notchedArrow = null;
	}

}
