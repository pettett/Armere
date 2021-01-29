using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;
public class Bow : SpawnableBody
{
	public Transform arrowAnchor;
	public Transform backHandAnchor;
	public Transform arrowSpawnPosition;
	Arrow notchedArrow;
	AmmoItemData currentAmmo;
	public async void NotchNextArrow(AmmoItemData ammo)
	{
		this.currentAmmo = ammo;
		//If an arrow is already notched, remove it and replaced
		if (notchedArrow != null) RemoveNotchedArrow();

		//create the arrow that should be fired next

		notchedArrow = (Arrow)await GameObjectSpawner.SpawnAsync(ammo.ammoGameObject, arrowAnchor);
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
		notchedArrow.Initialize(currentAmmo, velocity);

		notchedArrow = null;
	}

}
