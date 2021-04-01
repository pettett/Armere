﻿using System.Collections;
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

		bool onSurface = true;
		bool stopped = true;
		const string animatorVariable = "IsSwimming";
		bool holdingCrouchKey;

		DebugMenu.DebugEntry<int, float> entry;

		public Swimming(PlayerController c, SwimmingTemplate t) : base(c, t)
		{

			c.rb.useGravity = false;
			c.rb.drag = t.waterDrag;
			c.animationController.enableFeetIK = false;
			c.animator.SetBool(animatorVariable, true);

			waterTrail = MonoBehaviour.Instantiate(t.waterTrailPrefab, transform);
			//Place slightley above water to avoid z buffer clashing
			waterTrail.transform.localPosition = Vector3.up * (t.waterSittingDepth + 0.03f);

			waterTrailController = waterTrail.GetComponent<WaterTrailController>();

			entry = DebugMenu.CreateEntry("Player", "Hits: {0} Current Depth: {1}", 0, 0f);

			c.inputReader.crouchEvent += OnCrouch;


		}

		public override void End()
		{
			c.rb.useGravity = true;
			c.animator.SetBool(animatorVariable, false);

			DebugMenu.RemoveEntry(entry);

			waterTrailController.StopTrail();
			waterTrailController.DestroyOnFinish();

			c.inputReader.crouchEvent -= OnCrouch;

		}



		public override void FixedUpdate()
		{
			//Test to see if the player is still in deep water

			c.collider.height = t.colliderHeight;
			//c.animationController.pelvisOffset = t.pelvisOffset;


			float heightOffset = 2;
			int hits = Physics.RaycastNonAlloc(
				transform.position + new Vector3(0, heightOffset, 0),
				Vector3.down, waterHits, c.maxWaterStrideDepth + heightOffset,
				//Scan for water and ground
				c.m_groundLayerMask | c.m_waterLayerMask, QueryTriggerInteraction.Collide);


			float currentDepth = 0;

			if (hits == 2)
			{
				WaterController w = waterHits[0].collider.GetComponentInParent<WaterController>();
				Debug.Log(waterHits[0].collider.name);

				if (w != null)
				{
					//Hit water and ground
					currentDepth = waterHits[0].distance - waterHits[1].distance;
					if (currentDepth <= c.maxWaterStrideDepth)
					{
						//Within walkable water
						c.ChangeToState(c.defaultState);
					}
				}
			}
			//If underwater and going up but close to the surface, return to surface
			else if (
				!onSurface &&
				c.rb.velocity.y >= 0 &&
				hits > 0 &&
				waterHits[0].distance < heightOffset &&
				heightOffset - waterHits[0].distance < c.maxWaterStrideDepth)
				ChangeDive(false);

			if (onSurface && c.allCPs.Count > 0)
			{
				//Player is colliding with something
				for (int i = 0; i < c.allCPs.Count; i++)
				{
					//If about perpendicular to wall and facing towards the normal
					if (Mathf.Abs(Vector3.Dot(Vector3.up, c.allCPs[i].normal)) < 0.01f && Vector3.Dot(c.allCPs[i].normal, transform.forward) < -0.8f)
					{

						//Colliding against wall
						//Test to see if we can vault up it into walking
						Vector3 origin = transform.position + Vector3.up * (t.waterSittingDepth + c.collider.height) - c.allCPs[i].normal * c.collider.radius * 2;


						Debug.DrawLine(origin, origin + Vector3.down * c.collider.height * 1.05f, Color.blue, Time.deltaTime);
						//Also allow vault into shallow water
						if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, c.collider.height + c.maxWaterStrideDepth, c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
						 Vector3.Dot(Vector3.up, hit.normal) > c.m_maxGroundDot)
						{
							//Can move to hit spot
							transform.position = hit.point;

							c.ChangeToState(TransitionStateTemplate.GenerateTransition(0.2f, c.defaultState));
							return;
						}
					}
				}
			}


			if (DebugMenu.menuEnabled)
			{
				entry.value0 = hits;
				entry.value1 = currentDepth;

			}


			Vector3 playerDirection;

			if (onSurface)
			{
				playerDirection = GameCameras.s.TransformInput(c.inputReader.horizontalMovement);
				playerDirection.y = 0;
				playerDirection *= t.waterMovementForce * Time.fixedDeltaTime;
			}
			else
			{
				playerDirection = GameCameras.s.cameraTransform.TransformDirection(
					new Vector3(c.inputReader.horizontalMovement.x, 0, c.inputReader.horizontalMovement.y));
				//Move up seperatley to camera
				playerDirection.y += c.inputReader.verticalMovement;
				playerDirection.Normalize();

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

			for (int i = 0; i < c.currentWater.sources.Length; i++)
			{
				Vector3 dir = transform.position - c.currentWater.sources[i].transform.position;
				float sqrDist = Vector3.SqrMagnitude(dir);
				float r = c.currentWater.sources[i].radius;
				if (sqrDist < r * r)
				{
					float dist = Mathf.Sqrt(sqrDist);
					dir /= dist;
					force += dir * c.currentWater.sources[i].strength / dist;
				}
			}

			c.rb.AddForce(force);

			if (onSurface)
				//Always force player to be on water surface while simming
				transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position + Vector3.up * 1000) - Vector3.up * t.waterSittingDepth;
			else
				transform.position = c.currentWater.waterVolume.ClosestPoint(transform.position);

			//Transition to dive if space pressed
			if (onSurface && holdingCrouchKey)
			{
				ChangeDive(true);
				transform.position -= Vector3.up * c.maxWaterStrideDepth;
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
