using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
namespace Armere.PlayerController
{

	/// <summary> Climb climbable objects


	public class LadderClimb : MovementState<LadderClimbTemplate>
	{
		public override string StateName => "Climbing Ladder";
		Climbable ladder;
		float height;

		Vector3 currentNormal;
		Vector3 currentPosition;
		bool reachedLadderTop = false;
		float oldColliderHeight;

		Vector2 inputHorizontal => c.inputReader.horizontalMovement;
		public LadderClimb(PlayerMachine machine, LadderClimbTemplate t) : base(machine, t)
		{


			if (t.climbable != null)
				ladder = t.climbable;
			else
				throw new System.ArgumentException("A Ladder object must be supplied to this state");

			oldColliderHeight = c.collider.height;
			c.collider.height = t.climbingColliderHeight;
			switch (ladder.surfaceType)
			{
				case Climbable.ClimbableSurface.Line:
					//calculate height the player is at if they come at the ladder from above
					height = Mathf.Clamp(ladder.transform.InverseTransformPoint(transform.position).y, 0, ladder.ladderHeight - c.profile.m_standingHeight - 0.1f);
					transform.SetPositionAndRotation(GetLadderPos(height), ladder.transform.rotation);
					break;
				case Climbable.ClimbableSurface.Mesh:
					//Place player below climb-up threshold
					var point = ladder.GetClosestPointOnMesh(transform.position + c.WorldDown * c.collider.height * 1.1f);
					//Debug.Log(point.point);
					currentPosition = point.point;
					currentNormal = point.normal;
					MovePlayerToMesh(point.point, point.normal);

					break;
			}
			c.rb.isKinematic = true;
			c.animationController.enableFeetIK = false;

			c.animationController.TriggerTransition(c.transitionSet.ladderClimb);


			c.inputReader.jumpEvent += OnJump;
		}

		public Vector3 GetLadderPos(float h)
		{
			return ladder.LadderPosAtHeight(h, c.collider.radius * 2f);
		}

		public override void End()
		{
			//Dont do if manually moved
			c.collider.height = oldColliderHeight;
			//prepare for climb up animation
			c.animator.SetFloat(c.transitionSet.horizontal.id, 0);
			c.animator.SetFloat(c.transitionSet.vertical.id, 0);
			animator.applyRootMotion = true;

			if (reachedLadderTop)
			{
				c.animationController.TriggerTransition(c.transitionSet.climbUpFromLadder);
			}
			else
			{
				c.animationController.TriggerTransition(c.transitionSet.stepDownFromLadder);
			}

			c.inputReader.jumpEvent -= OnJump;
		}

		public int RoundToNearest(float value, int interval, int offset) => Mathf.RoundToInt((value - offset) / interval) * interval + offset;

		//   float lhRung, rhRung, lfRung, rfRung = 0;

		void UpdateRung(ref float rung, float height, int offset)
		{
			rung = Mathf.Lerp(rung, RoundToNearest(height, 2, offset), Time.deltaTime * 25);//Closest even rung
		}



		public override void OnAnimatorIK(int layerIndex)
		{
			// float rung = (height + handLadderHeight - ladder.rungOffset) / ladder.rungDistance;
			// //Update the position of every body part
			// UpdateRung(ref lhRung, rung, 0);
			// UpdateRung(ref rhRung, rung, 1);
			// //The height of feet is lower then the hands ; target a different position
			// rung = (height + footLadderHeight - ladder.rungOffset) / ladder.rungDistance;
			// UpdateRung(ref lfRung, rung, 0);
			// UpdateRung(ref rfRung, rung, 1);

			// SetPosition(AvatarIKGoal.LeftHand, "LeftHandCurve", ladder.LadderPosByRung(lhRung, -0.1f));
			// SetPosition(AvatarIKGoal.RightHand, "RightHandCurve", ladder.LadderPosByRung(rhRung, 0.1f));

			// //Do the same for feet
			// SetPosition(AvatarIKGoal.LeftFoot, "LeftFootCurve", ladder.LadderPosByRung(lfRung, -0.1f));
			// SetPosition(AvatarIKGoal.RightFoot, "RightFootCurve", ladder.LadderPosByRung(rfRung, 0.1f));
		}

		void SetPosition(AvatarIKGoal goal, string curve, Vector3 pos)
		{
			animator.SetIKPositionWeight(goal, animator.GetFloat(curve));
			animator.SetIKPosition(goal, pos);
		}
		public void OnJump(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Started)
			{
				//Jump off the ladder
				c.rb.isKinematic = false;
				c.rb.AddForce(-transform.forward * t.jumpForceHorizontal + transform.up * t.jumpForceVertical);
				c.machine.ChangeToState(machine.defaultState);

				transform.forward = -transform.forward;
			}
		}

		void MovePlayerToMesh(Vector3 pos, Vector3 normal)
		{
			transform.position = pos + normal * c.collider.radius;
			transform.forward = -normal;
		}
		float Dot(in Vector3 a, in Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
		public override void Update()
		{


			switch (ladder.surfaceType)
			{
				case Climbable.ClimbableSurface.Line:
					height += inputHorizontal.y * Time.deltaTime * t.climbingSpeed;
					height = Mathf.Clamp(height, 0, ladder.ladderHeight - c.profile.m_standingHeight);

					transform.position = GetLadderPos(height);

					if (height >= ladder.ladderHeight - c.profile.m_standingHeight - 0.01f)
					{
						//Get off at the top of the ladder
						transform.position = GetLadderPos(ladder.ladderHeight - c.profile.m_standingHeight);
						reachedLadderTop = true;
						c.machine.ChangeToState(TransitionStateTemplate.GenerateTransition(t.transitionTime, machine.defaultState));
					}

					animator.SetFloat(c.transitionSet.vertical.id, inputHorizontal.y * t.climbingSpeed);
					animator.SetFloat(c.transitionSet.horizontal.id, 0f);
					break;
				case Climbable.ClimbableSurface.Mesh:
					//Move with input on the mesh
					Vector3 leftTangent = -Vector3.Cross(c.WorldUp, currentNormal).normalized;
					Vector3 upTangent = Vector3.Cross(leftTangent, currentNormal);


					Vector3 deltaPos = (leftTangent * inputHorizontal.x + upTangent * inputHorizontal.y) * Time.deltaTime * t.climbingSpeed;
					currentPosition += deltaPos;

					var point = ladder.GetClosestPointOnMesh(currentPosition);

					float downwardsMovement = Dot(deltaPos, upTangent);
					float downwardsDelta = Dot(currentPosition - point.point, upTangent);
					bool hittingBase = downwardsMovement < 0 && downwardsDelta <= downwardsMovement * 0.5f;

					//print("delta {0}:real {1}: hitting {2}", deltaPos.y, currentPosition.y - point.point.y, hittingBase);

					//If moving downwards and no real movement
					if (hittingBase)
					{
						//Not able to go down further, test if we are standing on ground
						const float upOffset = 0.5f;
						const float minGroundDistance = 0.2f;
						Vector3 origin = currentPosition + c.WorldUp * upOffset + currentNormal * c.collider.radius;
						//Debug.DrawLine(origin, origin + c.WorldDown * (upOffset + minGroundDistance), Color.red, 1);
						if (Physics.Raycast(origin, c.WorldDown, upOffset + minGroundDistance, c.m_groundLayerMask, QueryTriggerInteraction.Ignore))
						{
							//Hit the ground layer, stand up from climbing
							c.machine.ChangeToState(machine.defaultState);
						}
					}

					currentPosition = point.point;
					currentNormal = point.normal;


					//Head position should be above center point, distance of collider away
					Quaternion rotation = Quaternion.LookRotation(currentNormal);

					//Test if were the head of the player is
					var headPoint = ladder.GetClosestPointOnMesh(currentPosition + upTangent * c.collider.height);


					var headPos = headPoint.point;
					Vector3 localHeadPos = rotation * (headPos - currentPosition);

					Vector3 flatHeadNormal = Vector3.ProjectOnPlane(headPoint.normal, c.WorldDown);
					Vector3 flatBodyNormal = Vector3.ProjectOnPlane(point.normal, c.WorldDown);

					float bodyHeadRotationDifference = Vector3.SignedAngle(flatHeadNormal, flatBodyNormal, c.WorldUp);
					if (Mathf.Abs(bodyHeadRotationDifference) > t.maxHeadBodyRotationDifference)
					{
						//go back left / right
						currentPosition.x -= deltaPos.x;
						currentPosition.z -= deltaPos.z;
					}

					float sqrHeadDistance = localHeadPos.y * localHeadPos.y + localHeadPos.z * localHeadPos.z;

					bool dirtyPosition = false;
					if (sqrHeadDistance < c.collider.height * c.collider.height)
					{
						float headDistance = Mathf.Sqrt(sqrHeadDistance);
						//print("Local head pos : {0} distance: {1}", localHeadPos.ToString("F3"), headDistance);
						//Cannot go this way, go back down or jump up to top surface

						Vector3 origin = currentPosition + c.WorldUp * (oldColliderHeight + c.collider.height) - currentNormal * c.collider.radius * 2;

						if (Physics.Raycast(origin, c.WorldDown, out RaycastHit hit, oldColliderHeight * 1.05f, c.m_groundLayerMask, QueryTriggerInteraction.Ignore) &&
							hit.distance > oldColliderHeight * 0.95f && Vector3.Dot(c.WorldUp, hit.normal) > c.profile.m_maxGroundSlopeDot)
						{
							//go to tops
							reachedLadderTop = true;
							c.machine.ChangeToState(TransitionStateTemplate.GenerateTransition(t.transitionTime, machine.defaultState));

						}
						else
						{
							//Go back down - no way to stand up on a top surface from here
							var offset = upTangent * (c.collider.height - headDistance);

							currentPosition -= offset;

							dirtyPosition = true;
						}
					}

					//measure from current point so we dont double- apply the limits
					if (Mathf.Abs(localHeadPos.x) > 0.01f)
					{
						//Go back on the player's x axis
						Vector3 diff = Quaternion.LookRotation(leftTangent) * Vector3.forward * -localHeadPos.x;
						currentPosition += diff;
						dirtyPosition = true;
					}

					if (dirtyPosition)
					{
						//refind the point for updated normal
						point = ladder.GetClosestPointOnMesh(currentPosition);
						currentPosition = point.point;
						currentNormal = point.normal;
					}

					Vector3 oldPos = transform.position;

					MovePlayerToMesh(currentPosition, currentNormal);

					deltaPos = transform.position - oldPos;

					//do normal dot product
					float up = Dot(upTangent, deltaPos);
					float left = Dot(leftTangent, deltaPos);

					//Get movement in the up and left tangent directions

					animator.SetFloat(c.transitionSet.vertical, Mathf.Sign(up) * Mathf.Clamp01(Mathf.Abs(up) + Mathf.Abs(left)) * t.meshAnimationSpeed);
					animator.SetFloat(c.transitionSet.horizontal, 0f);
					animator.SetBool(c.transitionSet.isGrounded, true);

					break;
			}
		}



#if UNITY_EDITOR
		public override void OnDrawGizmos()
		{

		}
#endif

	}

}