using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{
	public class PlayerBowAttack : Spell
	{
		float bowCharge = 0;
		readonly PlayerController c;
		readonly Walking w;
		readonly Bow bow;
		GameCameras cameras => GameCameras.s;

		bool bowAimViewEnabled = false;
		bool chargingBow = false;
		public PlayerBowAttack(PlayerController c, Walking w) : base(w)
		{
			this.c = c;
			this.w = w;
			bow = c.weaponGraphics.holdables.bow.gameObject.GetComponent<Bow>();




			EnableBowAimView();
		}
		public override void Begin()
		{

			c.inputReader.equipBowEvent += OnBowAttack;
		}
		void End()
		{
			//Dont do this, only stop bow view if player sheaths bow
			DisableBowAimView();
			c.animationController.headWeight = 0; // don't need to do others - master switch

			c.weaponGraphics.holdables.bow.holdPoint.overrideRig.bowPull = 0;
			bowCharge = 0;

			Debug.Log("Ending bow charging");

			c.inputReader.equipBowEvent -= OnBowAttack;
		}
		public void OnBowAttack(InputActionPhase phase)
		{

		}

		public void BeginCharge()
		{
			if (c.weaponGraphics.holdables.bow.sheathed)
			{
				bow.InitBow(c.inventory.bow.items[c.currentBow].itemData);
				c.NotchArrow(); //TODO: Notch arrow should play animation, along with drawing bow
				c.StartCoroutine(w.DrawItem(ItemType.Bow, () =>
				{
					chargingBow = true;
				}));
			}
			else
			{
				chargingBow = true;
			}

		}

		public void ReleaseBow()
		{

			if (bowCharge > 0.2f)
				FireBow();

			End();
		}

		public void FireBow()
		{
			if (bowCharge < 0.1f)
			{
				return; //To little charge
			}
			//print("Charged bow to {0}", bowCharge);
			//Fire ammo

			bow.ReleaseArrow((BowAimPos() - bow.arrowSpawnPosition.position).normalized * GetArrowSpeed(bowCharge));

			//Initialize arrow
			//Remove one of ammo used
			c.inventory.TakeItem(c.currentAmmo, ItemType.Ammo);

			//Test if ammo left for shooting
			if (c.inventory.ItemCount(c.currentAmmo, ItemType.Ammo) == 0)
			{
				Debug.Log($"Run out of arrow type {c.currentAmmo}");
				//Keep current ammo within range of avalibles
				c.SelectAmmo(c.currentAmmo);
			}
			else
			{
				c.NotchArrow();
			}
		}
		float GetArrowSpeed(float bowCharge) => Mathf.Lerp(w.t.arrowSpeedRange.x, w.t.arrowSpeedRange.y, bowCharge);

		void EnableBowAimView()
		{
			if (!bowAimViewEnabled)
			{
				cameras.SetCameraXOffset(w.t.shoulderViewXOffset);

				cameras.EnableFreeLookAimMode();

				w.t.onAimModeEnable.RaiseEvent();
				bowAimViewEnabled = true;
			}
		}
		void DisableBowAimView()
		{
			if (bowAimViewEnabled)
			{
				cameras.SetCameraXOffset(0);

				cameras.DisableFreeLookAimMode();

				w.t.onAimModeDisable.RaiseEvent();
				bowAimViewEnabled = false;
			}
		}
		Vector3 BowAimPos()
		{
			if (Physics.Raycast(cameras.cameraTransform.position, cameras.cameraTransform.forward, out RaycastHit hit))
			{
				//c.animationController.lookAtPosition = hit.point;
				return hit.point;
			}
			else
			{
				//c.animationController.lookAtPosition = cameras.cameraTransform.forward * 1000 + cameras.cameraTransform.position;
				return cameras.cameraTransform.position + cameras.cameraTransform.forward * 100;
			}
		}

		public void Cast()
		{
			FireBow();

			End();
		}

		public override void Update()
		{
			c.weaponGraphics.holdables.bow.holdPoint.overrideRig.lookAtTarget.position = BowAimPos();

			if (chargingBow)
			{
				bowCharge += Time.deltaTime;
				bowCharge = Mathf.Clamp01(bowCharge);
			}
			c.weaponGraphics.holdables.bow.holdPoint.overrideRig.bowPull = bowCharge;

		}

		public override void EndCast(bool manualCancel)
		{
			End();
		}


	}
}