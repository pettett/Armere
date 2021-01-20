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
	ItemName arrowName;
	public async void NotchNextArrow(ItemName arrow)
	{
		this.arrowName = arrow;
		//If an arrow is already notched, remove it and replaced
		if (notchedArrow != null) RemoveNotchedArrow();

		//create the arrow that should be fired next
		var ammo = (AmmoItemData)InventoryController.singleton.db[arrow];

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
		notchedArrow.Initialize(arrowName, velocity, InventoryController.singleton.db);

		notchedArrow = null;
	}

}
