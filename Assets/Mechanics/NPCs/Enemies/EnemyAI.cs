using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Armere.Inventory;

[RequireComponent(typeof(Health), typeof(WeaponGraphicsController), typeof(Ragdoller))]
public class EnemyAI : AIHumanoid, IExplosionEffector
{
	[System.NonSerialized] public Health health;

	[Header("Player Engagement")]

	public InvestigateRoutine investigate;
	public AlertRoutine alert;

	public float knockoutTime = 4f;
	public float maxExplosionKnockoutTime = 4f;
	[Header("Indicators")]
	public float height = 1.8f;



	Vector3 lookingAtOffset = Vector3.up * 1.6f;





	public void OnDamageTaken(GameObject attacker, GameObject victim)
	{
		//Push the ai back
		if (currentState.alertOnAttack && attacker.TryGetComponent(out Character c))
			ChangeToState(alert.EngageWith(c));
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

	public override void Start()
	{
		health = GetComponent<Health>();

		collider = GetComponent<Collider>();

		health.onTakeDamage += OnDamageTaken;
		health.onDeathEvent.AddListener(Die);

		base.Start();
		//TODO: remove
		meshController.SetMeshColor(Color.red);

		GetComponent<VirtualAudioListener>().onHearNoise += OnNoiseHeard;

		if (inventory.HasMeleeWeapon)
		{
			inventory.inventory.selectedMelee = inventory.BestMeleeWeapon;
			SetHeldMelee(inventory.SelectedMeleeWeapon);
		}
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
		//gameObject.SetActive(true);
	}





	public void ForceEngage(Character c)
	{
		ChangeToState(alert.EngageWith(c));
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
			Team[] enemies = Enemies();
			for (int i = 0; i < enemies.Length; i++)
			{
				if (teams.ContainsKey(enemies[i]))
				{
					List<Character> targets = teams[enemies[i]];
					for (int j = 0; j < targets.Count; j++)
					{
						var b = targets[i].bounds;
						if (ProportionBoundsVisible(b) != 0)
						{
							//can see the player, interrupt current routine
							ChangeToState(investigate.Investigate(targets[i]));
							return;
						}
					}
				}
			}
		}

		float speed = Mathf.Sign(agent.velocity.sqrMagnitude);

		animationController.anim.SetBool("Idle", speed == 0);
		animationController.anim.SetFloat("InputVertical", agent.velocity.sqrMagnitude / (agent.speed * agent.speed), 0.01f, Time.deltaTime);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (lookingAtTarget != null)
			LookAtPlayer(lookingAtTarget.position + lookingAtOffset);
	}

	private void OnDrawGizmos()
	{


		Team[] enemies = Enemies();
		for (int i = 0; i < enemies.Length; i++)
		{
			if (teams.ContainsKey(enemies[i]))
			{
				List<Character> targets = teams[enemies[i]];
				for (int j = 0; j < targets.Count; j++)
				{
					var b = targets[j].bounds;

					float visibility = ProportionBoundsVisible(b);
					if (CanSeeBounds(b))
					{
						Gizmos.color = new Color(visibility, 0, 0); Gizmos.DrawWireCube(b.center, b.size);
					}



				}
			}
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