using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class CutLog : MonoBehaviour, IAttackable
{
	public GameObject canopy;
	public GameObject trunk;

	//Needed to spawn items and place offset
	public Vector2 lengthRegion;


	public Vector3 offset;
	Vector3 IScanable.offset => offset;


	public bool invincible = true;
	public IEnumerator Start()
	{
		offset = Vector3.up * (lengthRegion.x + lengthRegion.y) * 0.5f;
		yield return new WaitForSeconds(0.5f); //Wait a short time before the log can be destroyed
		invincible = false;
	}

	public void Cut()
	{

		Destroy(gameObject);
	}

	public AttackResult Attack(DamageType flags, float damage, GameObject controller, Vector3 hitPosition)
	{
		if (!invincible)
		{
			Cut();
			return AttackResult.Damaged | AttackResult.Killed;
		}
		else
		{
			return AttackResult.None;
		}
	}

	private void OnEnable()
	{
		TypeGroup<IAttackable>.allObjects.Add(this);
	}
	private void OnDisable()
	{
		TypeGroup<IAttackable>.allObjects.Remove(this);
	}

	//Remove the canopy when hit ground
	private void OnCollisionEnter(Collision other)
	{
		if (other.gameObject != trunk && (other.rigidbody?.isKinematic ?? true))
		{
			//Only interact with kinematic rbs or nothing
			Destroy(canopy);
		}
	}
}
