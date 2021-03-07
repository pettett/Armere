using System.Collections;
using System.Collections.Generic;
using Armere.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "Auto Bow Routine", menuName = "Game/NPCs/Auto Bow Routine", order = 0)]
public class AutoBowRoutine : EnemyRoutine
{
	public override bool alertOnAttack => false;

	public override bool searchOnEvent => false;

	public override bool investigateOnSight => false;
	public BowItemData bowItem;
	public AmmoItemData ammoItem;
	public float bowFireRate = 1;
	public float arrowNotchTime = 0.1f;
	public float arrowSpeed = 100;
	public override IEnumerator Routine(EnemyAI enemy)
	{
		var arrowNotch = new WaitForSeconds(arrowNotchTime);
		//Shoot the bow forever

		var task = enemy.SetHeldBow(bowItem);
		while (!task.IsCompleted) yield return null;

		enemy.weaponGraphics.holdables.bow.sheathed = false;

		float arrowChargeTime = 1 / bowFireRate - arrowNotchTime;
		var bowAC = enemy.weaponGraphics.holdables.bow.gameObject.GetComponent<Animator>();

		var bow = enemy.weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>();
		bow.InitBow(bowItem);

		enemy.animationController.lookAtPositionWeight = 1;
		enemy.animationController.headLookAtPositionWeight = 1;
		enemy.animationController.eyesLookAtPositionWeight = 1;
		enemy.animationController.bodyLookAtPositionWeight = 1;
		enemy.animationController.clampLookAtPositionWeight = 0.5f; //180 degrees
		enemy.animationController.TriggerTransition(enemy.transitionSet.holdBow);

		while (true)
		{
			float t = 0;


			bow.NotchNextArrow(ammoItem);

			while (t < 1)
			{
				t += Time.deltaTime;
				bowAC.SetFloat("Charge", t);

				bow.transform.forward = enemy.transform.forward;
				enemy.animationController.lookAtPosition = bow.arrowSpawnPosition.position + enemy.transform.forward * arrowSpeed;

				yield return null;
			}

			bow.ReleaseArrow(enemy.transform.forward * arrowSpeed);

			yield return arrowNotch;
		}
	}


	// IEnumerator ChargeBow()
	// {
	// 	float bowCharge = 0;
	// 	bool chargingBow = true;

	// 	void ReleaseBow()
	// 	{

	// 		if (bowCharge > 0.2f)
	// 			FireBow();

	// 		End();
	// 	}

	// 	void FireBow()
	// 	{
	// 		//print("Charged bow to {0}", bowCharge);
	// 		//Fire ammo
	// 		var bow = c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>();
	// 		bow.ReleaseArrow(GameCameras.s.cameraTransform.forward * GetArrowSpeed(bowCharge));

	// 		//Initialize arrow
	// 		//Remove one of ammo used
	// 		c.inventory.TakeItem(c.currentAmmo, ItemType.Ammo);

	// 		//Test if ammo left for shooting
	// 		if (c.inventory.ItemCount(c.currentAmmo, ItemType.Ammo) == 0)
	// 		{
	// 			Debug.Log($"Run out of arrow type {c.currentAmmo}");
	// 			//Keep current ammo within range of avalibles
	// 			c.SelectAmmo(c.currentAmmo);
	// 		}
	// 		else
	// 		{
	// 			c.NotchArrow();
	// 		}
	// 	}
	// 	void End()
	// 	{

	// 		DisableBowAimView();

	// 		forceForwardHeadingToCamera = false;
	// 		c.animationController.lookAtPositionWeight = 0; // don't need to do others - master switch
	// 		c.animator.SetBool("Holding Bow", false);
	// 		c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Animator>().SetFloat("Charge", 0);



	// 		OnReleaseBowEvent -= ReleaseBow;
	// 		OnCancelBowEvent -= End;
	// 		chargingBow = false; //Ends the loop
	// 	}



	// 	if (c.weaponGraphicsController.holdables.bow.sheathed)
	// 	{
	// 		c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Bow>().InitBow(c.inventory.bow.items[c.currentBow].itemData);
	// 		c.NotchArrow(); //TODO: Notch arrow should play animation, along with drawing bow
	// 		yield return DrawItem(ItemType.Bow);
	// 	}

	// 	OnReleaseBowEvent += ReleaseBow;
	// 	OnCancelBowEvent += End;

	// 	forceForwardHeadingToCamera = true;


	// 	EnableBowAimView();

	// 	c.animator.SetBool("Holding Bow", true);

	// 	c.animationController.lookAtPositionWeight = 1;
	// 	c.animationController.headLookAtPositionWeight = 1;
	// 	c.animationController.eyesLookAtPositionWeight = 1;
	// 	c.animationController.bodyLookAtPositionWeight = 1;
	// 	c.animationController.clampLookAtPositionWeight = 0.5f; //180 degrees

	// 	var bowAC = c.weaponGraphicsController.holdables.bow.gameObject.GetComponent<Animator>();
	// 	while (chargingBow)
	// 	{
	// 		yield return null;
	// 		if (Physics.Raycast(GameCameras.s.cameraTransform.position, GameCameras.s.cameraTransform.forward, out RaycastHit hit))
	// 		{
	// 			c.animationController.lookAtPosition = hit.point;
	// 			c.weaponGraphicsController.holdables.bow.gameObject.transform.LookAt(hit.point);
	// 		}
	// 		else
	// 		{
	// 			c.animationController.lookAtPosition = GameCameras.s.cameraTransform.forward * 1000 + GameCameras.s.cameraTransform.position;
	// 			c.weaponGraphicsController.holdables.bow.gameObject.transform.forward = GameCameras.s.cameraTransform.forward;
	// 		}


	// 		bowCharge += Time.deltaTime;
	// 		bowCharge = Mathf.Clamp01(bowCharge);
	// 		bowAC.SetFloat("Charge", bowCharge);
	// 		//Update trajectory (in local space)
	// 	}
	// }
}