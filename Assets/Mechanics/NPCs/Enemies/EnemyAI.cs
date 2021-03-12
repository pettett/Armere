using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Armere.Inventory;

[RequireComponent(typeof(Health), typeof(WeaponGraphicsController), typeof(Ragdoller))]
public class EnemyAI : AIHumanoid, IExplosionEffector
{



	[HideInInspector] public Health health;
	[Header("Player Detection")]

	Collider playerCollider;



	[Header("Player Engagement")]

	public InvestigateRoutine investigate;

	public float knockoutTime = 4f;
	public float maxExplosionKnockoutTime = 4f;
	[Header("Indicators")]
	public float height = 1.8f;
	AlertIndicatorUI alert;



	Vector3 lookingAtOffset = Vector3.up * 1.6f;







	public void OnDamageTaken(GameObject attacker, GameObject victim)
	{
		//Push the ai back
		if (currentState.alertOnAttack)
			ChangeToState(new AlertRoutine(this, 1, playerCollider));
	}

	public void OnExplosion(Vector3 source, float radius, float force)
	{

		float sqrDistance01 = 1 - Vector3.SqrMagnitude(transform.position - source) / (radius * radius);


		ChangeToState(new KnockoutRoutine(this, sqrDistance01 * maxExplosionKnockoutTime));

	}

	[MyBox.ButtonMethod()]
	public void Ragdoll()
	{
		ChangeToState(new KnockoutRoutine(this, knockoutTime));
	}


	public override void Knockout(float time)
	{
		//TODO: Make this better
		health.Damage(time * 10, gameObject);
		ChangeToState(new KnockoutRoutine(this, time));
	}

	private void OnValidate()
	{
		if (clippingPlanes.x > clippingPlanes.y)
		{
			//If the lower value is bigger, make the upper value equal
			clippingPlanes.y = clippingPlanes.x;
		}
		else if (clippingPlanes.y < clippingPlanes.x)
		{
			//if the upper value is smaller, make the lower value equal
			clippingPlanes.x = clippingPlanes.y;
		}
	}

	public void Die()
	{
		ChangeToState(new DieRoutine(this));
	}

	protected override async void Start()
	{
		playerCollider = LevelInfo.currentLevelInfo.player.GetComponent<Collider>();

		health = GetComponent<Health>();
		weaponGraphics = GetComponent<WeaponGraphicsController>();
		animationController = GetComponent<AnimationController>();


		health.onTakeDamage += OnDamageTaken;
		health.onDeathEvent.AddListener(Die);

		base.Start();

		GetComponent<VirtualAudioListener>().onHearNoise += OnNoiseHeard;

		if (meleeWeapon != null)
			await SetHeldMelee(meleeWeapon);
	}


	public void OnNoiseHeard(Vector3 position)
	{
		if (currentState.searchOnEvent)
		{
			ChangeToState(new SearchForEventRoutine(this, position));
		}
	}




	public virtual void InitEnemy()
	{
		gameObject.SetActive(true);
	}

	public class DieRoutine : AIState
	{
		public override bool alertOnAttack => false;

		public override bool searchOnEvent => false;

		public override bool investigateOnSight => false;
		Coroutine r;
		public DieRoutine(AIHumanoid c) : base(c)
		{

			r = c.StartCoroutine(Routine());
		}
		public override void End()
		{
			c.StopCoroutine(r);
		}
		IEnumerator Routine()
		{

			foreach (var x in c.weaponGraphics.holdables)
				x.RemoveHeld();

			c.ragdoller.RagdollEnabled = true;
			yield return new WaitForSeconds(4);
			Destroy(c.gameObject);
		}
	}






	public class KnockoutRoutine : AIState
	{
		public override bool alertOnAttack => false;

		public override bool searchOnEvent => false;

		public override bool investigateOnSight => false;

		readonly float knockoutTime;


		Coroutine r;
		public KnockoutRoutine(AIHumanoid c, float knockoutTime) : base(c)
		{
			this.knockoutTime = knockoutTime;
			r = c.StartCoroutine(Routine());
		}
		public override void End()
		{
			c.StopCoroutine(r);
		}

		IEnumerator Routine()
		{
			c.ragdoller.RagdollEnabled = true;
			c.GetComponent<Focusable>().enabled = false;
			yield return new WaitForSeconds(knockoutTime);
			c.GetComponent<Focusable>().enabled = true;
			c.ragdoller.RagdollEnabled = false;
		}
	}


	public class AlertRoutine : AIState
	{
		public override bool alertOnAttack => false;

		public override bool searchOnEvent => false;

		public override bool investigateOnSight => false;

		public float waitTime;

		Coroutine r;
		Collider playerCollider;
		public AlertRoutine(AIHumanoid c, float waitTime, Collider playerCollider) : base(c)
		{
			this.waitTime = waitTime;
			this.playerCollider = playerCollider;
			r = c.StartCoroutine(Routine());
		}
		public override void End()
		{
			c.StopCoroutine(r);
		}

		IEnumerator Routine()
		{

			c.lookingAtTarget = playerCollider.transform;

			c.OnPlayerDetected();


			c.animationController.TriggerTransition(c.transitionSet.surprised);
			yield return new WaitForSeconds(1);

			if (c.weaponGraphics.holdables.melee.sheathed)
				yield return c.DrawItem(ItemType.Melee);


			//If alert is null create one
			//c.alert = c.alert ?? IndicatorsUIController.singleton.CreateAlertIndicator(c.transform, Vector3.up * c.height);

			//c.alert.EnableInvestigate(false);
			//c.alert.EnableAlert(true);
			yield return new WaitForSeconds(1);
			//print("Alerted");
			//Destroy(c.alert.gameObject);
			c.ChangeToState(new EngagePlayerRoutine(c, playerCollider));
		}
	}
	public void ForceEngage()
	{
		ChangeToState(new AlertRoutine(this, 0, playerCollider));
	}

	public class SearchForEventRoutine : AIState
	{
		public override bool alertOnAttack => true;

		public override bool searchOnEvent => true;

		public override bool investigateOnSight => true;
		readonly Vector3 eventPos;
		Coroutine r;
		public SearchForEventRoutine(AIHumanoid c, Vector3 eventPos) : base(c)
		{
			this.eventPos = eventPos;
			r = c.StartCoroutine(Routine());
		}
		public override void End()
		{
			c.StopCoroutine(r);
		}
		IEnumerator Routine()
		{

			/*
            Investigate routine:
                Go to close enough distance to event
                Rotate to event
                Wait there, looking around a bit
                go back to what we were doing before
            */

			c.debugText.SetText("Searching");
			yield return c.RotateTo(Quaternion.LookRotation(eventPos - c.transform.position), c.agent.angularSpeed);
			c.debugText.SetText("Searching - looking");
			yield return new WaitForSeconds(3);
		}
	}
	public class EngagePlayerRoutine : AIState
	{
		public override bool alertOnAttack => false;

		public override bool searchOnEvent => false;

		public override bool investigateOnSight => false;
		public float approachDistance = 1;
		float sqrApproachDistance => approachDistance * approachDistance;
		public bool approachPlayer = true;
		Coroutine r;
		Collider playerCollider;
		public EngagePlayerRoutine(AIHumanoid c, Collider playerCollider) : base(c)
		{
			r = c.StartCoroutine(Routine());
			this.playerCollider = playerCollider;
		}
		public override void End()
		{
			c.StopCoroutine(r);
		}

		IEnumerator Routine()
		{
			//Once they player has attacked or been seen, do not stop engageing until circumstances change
			c.agent.isStopped = true;

			Vector3 directionToPlayer;
			Health playerHealth = playerCollider.GetComponent<Health>();
			bool movingToCatchPlayer = false;
			c.lookingAtTarget = playerCollider.transform;
			//Stop attacking the player after it has died
			while (!playerHealth.dead)
			{
				directionToPlayer = playerCollider.transform.position - c.transform.position;
				if (approachPlayer && directionToPlayer.sqrMagnitude > sqrApproachDistance)
				{
					if (!movingToCatchPlayer)
					{
						movingToCatchPlayer = true;
						yield return new WaitForSeconds(0.1f);
					}

					c.agent.Move(directionToPlayer.normalized * Time.deltaTime * c.agent.speed);
				}
				else if (movingToCatchPlayer)
				{
					movingToCatchPlayer = false;
					//Small delay to adjust to stopped movement
					yield return new WaitForSeconds(0.1f);
				}
				else
				{
					//Within sword range of player
					//Swing sword
					yield return c.SwingSword();
				}

				directionToPlayer.y = 0;
				c.transform.forward = directionToPlayer;

				//TODO: Test to see if the player is still in view


				yield return null;
			}
			//Once the player has died, return to normal routine to stop end looking janky
			c.ChangeToState(c.defaultState);
		}

	}





	public bool CanSeeBounds(Bounds b)
	{
		if (sightMode == SightMode.View)
		{
			var viewMatrix = Matrix4x4.Perspective(fov, 1, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1));
			GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, viewPlanes);
			return GeometryUtility.TestPlanesAABB(viewPlanes, b);
		}
		else if (sightMode == SightMode.Range)
		{
			float sqrDistance = (b.center - eye.position).sqrMagnitude;
			if (sqrDistance < clippingPlanes.y * clippingPlanes.y && sqrDistance > clippingPlanes.x * clippingPlanes.x)
			{
				return true;
			}
			else return false;
		}
		return false;
	}


	protected override void Update()
	{
		base.Update();
		//Test if the c can see the player at this point
		if (currentState?.investigateOnSight ?? false)
		{
			var b = playerCollider.bounds;
			if (ProportionBoundsVisible(b) != 0)
			{
				//can see the player, interrupt current routine
				ChangeToState(investigate.Investigate(playerCollider));
			}
		}

		float speed = Mathf.Sign(agent.velocity.sqrMagnitude);

		anim.SetBool("Idle", speed == 0);
		anim.SetFloat("InputVertical", agent.velocity.sqrMagnitude / (agent.speed * agent.speed), 0.01f, Time.deltaTime);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (lookingAtTarget != null)
			LookAtPlayer(lookingAtTarget.position + lookingAtOffset);
	}

	private void OnDrawGizmos()
	{

		if (playerCollider != null)
		{
			var b = playerCollider.bounds;

			float visibility = ProportionBoundsVisible(b);
			if (CanSeeBounds(b))
			{
				Gizmos.color = new Color(visibility, 0, 0);
				Gizmos.DrawWireCube(b.center, b.size);
			}



			if (sightMode == SightMode.View)
			{
				Gizmos.color = Color.white;
				Gizmos.matrix = eye.localToWorldMatrix;
				Gizmos.DrawFrustum(Vector3.zero, fov, clippingPlanes.y, clippingPlanes.x, 1f);
			}
			else if (sightMode == SightMode.Range)
			{
				Gizmos.DrawWireSphere(eye.position, clippingPlanes.x);
				Gizmos.DrawWireSphere(eye.position, clippingPlanes.y);
			}
		}
	}
}