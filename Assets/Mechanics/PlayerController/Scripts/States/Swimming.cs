using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{
	public class Swimming : MovementState<SwimmingTemplate>
	{
		public override string StateName => "Swimming";
		readonly GameObject waterTrail;
		readonly WaterTrailController waterTrailController;
		readonly RaycastHit[] waterHits = new RaycastHit[2];
		readonly WaterController fluidController;

		bool onSurface = true;
		bool stopped = true;
		const string animatorVariable = "IsSwimming";
		bool holdingCrouchKey;
		float oldDrag;
		System.Text.StringBuilder entry;

		public Swimming(PlayerMachine machine, SwimmingTemplate t) : base(machine, t)
		{
			c.useGravity = false;
			(oldDrag, c.rb.drag) = (c.rb.drag, t.waterDrag);

			c.rb.velocity = Vector3.ProjectOnPlane(c.rb.velocity, c.WorldDown);

			c.animationController.enableFeetIK = false;
			c.animator.SetBool(animatorVariable, true);

			fluidController = c.GetComponent<PlayerWaterObject>().currentFluid;

			waterTrail = MonoBehaviour.Instantiate(t.waterTrailPrefab, transform);
			//Place slightley above water to avoid z buffer clashing
			waterTrail.transform.localPosition = c.WorldUp * (t.waterSittingDepth + 0.03f);

			waterTrailController = waterTrail.GetComponent<WaterTrailController>();

			entry = DebugMenu.CreateEntry("Player");

			c.inputReader.crouchEvent += OnCrouch;
		}

		public override void End()
		{
			c.useGravity = true;
			c.animator.SetBool(animatorVariable, false);

			DebugMenu.RemoveEntry(entry);

			waterTrailController.StopTrail();
			waterTrailController.DestroyOnFinish();

			c.inputReader.crouchEvent -= OnCrouch;

			c.rb.drag = oldDrag;

		}



		public override void FixedUpdate()
		{
			//Test to see if the player is still in deep water

			c.collider.height = t.colliderHeight;
			//c.animationController.pelvisOffset = t.pelvisOffset;


			float heightOffset = 2;
			int hits = Physics.RaycastNonAlloc(
				transform.position + new Vector3(0, heightOffset, 0),
				Vector3.down, waterHits, c.profile.maxWaterStrideDepth + heightOffset,
				//Scan for water and ground
				c.m_groundLayerMask | c.m_waterLayerMask, QueryTriggerInteraction.Collide);


			float currentDepth = 0;

			if (hits == 2)
			{
				WaterController w = waterHits[0].collider.GetComponentInParent<WaterController>();
				if (w != null)
				{
					//Hit water and ground
					currentDepth = waterHits[0].distance - waterHits[1].distance;
					if (currentDepth <= c.profile.maxWaterStrideDepth)
					{
						//Within walkable water
						machine.ChangeToState(machine.defaultState);
					}
				}
			}
			//If underwater and going up but close to the surface, return to surface
			else if (
				!onSurface &&
				c.rb.velocity.y >= 0 &&
				hits > 0 &&
				waterHits[0].distance < heightOffset &&
				heightOffset - waterHits[0].distance < c.profile.maxWaterStrideDepth)
				ChangeDive(false);

			if (onSurface && c.allCPs.Count > 0)
			{
				//Player is colliding with something
				for (int i = 0; i < c.allCPs.Count; i++)
				{
					//If about perpendicular to wall and facing towards the normal
					if (Mathf.Abs(Vector3.Dot(c.WorldUp, c.allCPs[i].normal)) < 0.01f && Vector3.Dot(c.allCPs[i].normal, transform.forward) < -0.8f)
					{

						//Colliding against wall
						//Test to see if we can vault up it into walking
						Vector3 origin = transform.position + c.WorldUp * (t.waterSittingDepth + c.collider.height) - c.allCPs[i].normal * c.collider.radius * 2;


						Debug.DrawLine(origin, origin + Vector3.down * c.collider.height * 1.05f, Color.blue, Time.deltaTime);
						//Also allow vault into shallow water
						if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, c.collider.height + c.profile.maxWaterStrideDepth, c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
						 Vector3.Dot(c.WorldUp, hit.normal) > c.profile.m_maxGroundSlopeDot)
						{
							//Can move to hit spot
							transform.position = hit.point;

							machine.ChangeToState(TransitionStateTemplate.GenerateTransition(0.2f, machine.defaultState));
							return;
						}
					}
				}
			}


			if (DebugMenu.menuEnabled)
			{
				entry.Clear();
				entry.AppendFormat("Hits: {0} Current Depth: {1}", hits, currentDepth);

			}


			Vector3 playerDirection;

			if (onSurface)
			{
				playerDirection = PlayerInputUtility.WorldSpaceFlatInput(c);
				playerDirection *= t.waterMovementForce * Time.fixedDeltaTime;
			}
			else
			{
				playerDirection = PlayerInputUtility.WorldSpaceFullInput(c);
				//Move up seperatley to camera

				playerDirection *= t.waterMovementForce * Time.fixedDeltaTime;
			}

			if (playerDirection.sqrMagnitude > 0)
			{
				transform.forward = playerDirection;
				if (onSurface && stopped)
				{
					stopped = false;
					waterTrailController.StartTrail();
				}
			}
			else if (onSurface && !stopped)
			{
				stopped = true;
				waterTrailController.StopTrail();
			}

			Vector3 force = playerDirection * t.waterMovementSpeed - c.rb.velocity;

			force = Vector3.ClampMagnitude(force, t.waterMovementForce * Time.fixedDeltaTime);


			//Add effects of water flow sources

			for (int i = 0; i < fluidController.sources.Length; i++)
			{
				Vector3 dir = transform.position - fluidController.sources[i].transform.position;
				float sqrDist = Vector3.SqrMagnitude(dir);
				float r = fluidController.sources[i].radius;
				if (sqrDist < r * r)
				{
					float dist = Mathf.Sqrt(sqrDist);
					dir /= dist;
					force += dir * fluidController.sources[i].strength / dist;
				}
			}

			c.rb.AddForce(force);

			if (onSurface)
				//Always force player to be on water surface while simming
				transform.position = fluidController.fluidVolume.ClosestPoint(transform.position + c.WorldUp * 1000) + c.WorldDown * t.waterSittingDepth;
			else
				transform.position = fluidController.fluidVolume.ClosestPoint(transform.position);

			//Transition to dive if space pressed
			if (onSurface && holdingCrouchKey)
			{
				ChangeDive(true);
				transform.position += c.WorldDown * c.profile.maxWaterStrideDepth;
			}
		}

		public void OnCrouch(InputActionPhase phase)
		{
			holdingCrouchKey = InputReader.PhaseMeansPressed(phase);
		}
		public override void Update()
		{
			animator.SetFloat(c.transitionSet.horizontal.id, 0);
		}


		void ChangeDive(bool diving)
		{
			onSurface = !diving;
			animator.SetBool(c.transitionSet.isGrounded.id, onSurface);
			if (onSurface) transform.rotation = Quaternion.identity;

			if (diving)
			{
				waterTrailController.StopTrail();
				//Make camera orbit around center
				GameCameras.s.playerTrackingOffset = 0;
				GameCameras.s.playerRigOffset = 1.6f;
			}
			else
			{

				GameCameras.s.playerRigOffset = GameCameras.s.defaultRigOffset;
				GameCameras.s.playerTrackingOffset = GameCameras.s.defaultTrackingOffset;
			}

		}


	}
}
