using System.Collections;
using UnityEngine;

namespace Armere.PlayerController
{
	public class PlayerHoldableInteraction : Spell
	{
		readonly HoldableBody holdable;
		readonly Walking walking;

		public PlayerHoldableInteraction(Walking walking, HoldableBody holdable) : base(walking)
		{
			this.holdable = holdable;
			this.walking = walking;
			caster.c.StartCoroutine(PickupHoldable());
		}
		IEnumerator PickupHoldable()
		{
			walking.inControl = false;
			yield return walking.c.UnEquipAll();




			(walking.machine.GetState(typeof(Interact)) as Interact).End();

			float rotationTime = 0.2f;
			var dir = holdable.transform.position - caster.transform.position;
			dir.y = 0;
			dir.Normalize();

			//Debug.Log(dir);

			Debug.DrawLine(caster.transform.position, caster.transform.position + dir * 5, Color.red, 5);
			Quaternion r = Quaternion.LookRotation(dir, Vector3.up);
			float angle = r.eulerAngles.y;

			caster.gameObject.LeanRotateY(angle, rotationTime);
			//caster.transform.rotation = r;
			yield return new WaitForSeconds(rotationTime);

			if (holdable.shape == HoldableBody.HoldableShape.Cylinder)
			{
				holdable.rb.isKinematic = true;
				holdable.collider.enabled = false;

				Vector3 start = holdable.transform.position;
				Quaternion startRot = holdable.transform.rotation;

				float t = 0;
				while (t < 1)
				{
					t += Time.deltaTime;
					walking.c.animationController.holdBarrelRig.SetWeight(t);
					holdable.transform.position = Vector3.Lerp(start, walking.c.animationController.holdBarrelRig.holderPosition, t);
					holdable.transform.rotation = Quaternion.Slerp(startRot, walking.c.animationController.holdBarrelRig.holderRotation, t);
					yield return null;
				}

				caster.c.animationController.holdBarrelRig.AttachBarrel(holdable.gameObject);

			}
			else
			{
				//Keep body attached to top of player;
				holdable.transform.position = (caster.transform.position + Vector3.up * (walking.walkingHeight + holdable.heightOffset));

				holdable.joint.connectedBody = walking.c.rb;
			}

			UIKeyPromptGroup.singleton.ShowPrompts(
				walking.c.inputReader,
				InputReader.groundActionMap,
				("Throw", InputReader.GroundActionMapActions.Attack),
				("Drop", InputReader.GroundActionMapActions.AltAttack));

			walking.inControl = true;
		}

		public override void Begin()
		{
			walking.forceForwardHeadingToCamera = false;
		}

		public override void EndCast(bool manualCancel)
		{
			if (manualCancel)
				PlaceHoldable();
			else
				DropHoldable();
		}

		public void Cast()
		{
			ThrowHoldable();
		}

		public override void Update()
		{
		}
		#region Holdables
		public void PlaceHoldable()
		{
			RemoveHoldable(Vector3.zero);
		}
		public void DropHoldable()
		{
			RemoveHoldable(Vector3.zero);
		}
		public void ThrowHoldable()
		{
			RemoveHoldable((caster.transform.forward + Vector3.up).normalized * walking.t.throwForce);
		}

		void RemoveHoldable(Vector3 acceleration)
		{

			if (holdable.shape == HoldableBody.HoldableShape.Cylinder)
			{
				caster.c.animationController.holdBarrelRig.DetachBarrel();
				holdable.transform.SetParent(null);
				holdable.rb.isKinematic = false;
				holdable.collider.enabled = true;
			}


			holdable.OnDropped();
			holdable.rb.AddForce(acceleration, ForceMode.Acceleration);


			UIKeyPromptGroup.singleton.RemovePrompts();

			(walking.machine.GetState(typeof(Interact)) as Interact).Start();
		}



		#endregion

	}
}