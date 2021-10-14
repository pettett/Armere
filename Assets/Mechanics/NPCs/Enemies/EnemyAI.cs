using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Armere.Inventory;

[RequireComponent(typeof(Health), typeof(Ragdoller))]
public class EnemyAI : AIHumanoid, IExplosionEffector
{
	[System.NonSerialized] public Health health;

	[Header("Player Engagement")]

	public AlertRoutine alert;

	public float knockoutTime = 4f;
	public float maxExplosionKnockoutTime = 4f;
	[Header("Indicators")]
	public float height = 1.8f;



	Vector3 lookingAtOffset = Vector3.up * 1.6f;



	public void OnExplosion(Vector3 source, float radius, float force)
	{

		float sqrDistance01 = 1 - Vector3.SqrMagnitude(transform.position - source) / (radius * radius);


		machine.ChangeToState(new KnockoutRoutine(machine, sqrDistance01 * maxExplosionKnockoutTime));

	}

	[MyBox.ButtonMethod()]
	public void Ragdoll()
	{
		machine.ChangeToState(new KnockoutRoutine(machine, knockoutTime));
	}


	public override void Knockout(float time)
	{
		//TODO: Make this better
		health.Damage(time * 10, gameObject);
		machine.ChangeToState(new KnockoutRoutine(machine, time));
	}


	public void Die()
	{
		machine.ChangeToState(new DieRoutine(machine));
	}

	public override void Start()
	{
		AssertComponent(out health);
		AssertComponent(out collider);

		health.onDeathEvent.AddListener(Die);

		base.Start();
		//TODO: remove
		meshController.SetMeshColor(Color.red);

		if (hasInventory && inventory.HasMeleeWeapon)
		{
			inventory.selectedMelee = inventory.BestMeleeWeapon;
			SetHeldMelee(inventory.SelectedMeleeWeapon);
		}
	}


	public virtual void InitEnemy()
	{
		//gameObject.SetActive(true);
	}




	public void ForceEngage(Character c)
	{
		machine.ChangeToState(alert.Target(c));
	}






	protected override void Update()
	{
		base.Update();
		//Test if the c can see the player at this point


		float speed = Mathf.Sign(velocity.sqrMagnitude);

		animationController.anim.SetBool("Idle", speed == 0);
		animationController.anim.SetFloat("InputVertical", velocity.sqrMagnitude / (agent.speed * agent.speed), 0.01f, Time.deltaTime);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (lookingAtTarget != null)
			LookAtPlayer(lookingAtTarget.position + lookingAtOffset);
	}


}