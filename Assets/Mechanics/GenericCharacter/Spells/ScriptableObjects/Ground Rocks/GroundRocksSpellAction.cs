using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundRocksSpell : Spell
{

	public readonly GroundRocksSpellAction a;

	public GroundRocksSpell(Character caster, GroundRocksSpellAction action) : base(caster)
	{
		a = action;

		UIKeyPromptGroup.singleton.ShowPrompts(a.input, InputReader.groundActionMap, ("Raise Rocks", InputReader.attackAction));

	}

	void End()
	{
		HidePrompts();
	}

	public override void CancelCast(bool manualCancel)
	{
		End();
	}

	public override void Cast()
	{


		Vector3 start = caster.transform.position;
		Vector3 dir = caster.transform.forward;
		Vector3 right = caster.transform.right;
		Vector3 pos;
		float gap = a.range / a.createdRocks;
		for (int i = 0; i < a.createdRocks; i++)
		{
			float dist = gap * i + a.initialRockDistance;

			float horizontal = Random.Range(-1f, 1f);


			pos = start + dir * dist + right * a.horizontalMovementRange * horizontal;

			if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1, -1))
			{
				//Make rocks follow ground
				pos = hit.position;
			}
			float forwardAngle = Random.Range(-a.forwardAngleRange, a.forwardAngleRange) + a.rockForwardTiltOverDistance.Evaluate(dist);

			Quaternion rot = Quaternion.Euler(forwardAngle, caster.transform.eulerAngles.y, -a.horizontalAngleRange * horizontal);

			float heightAboveGround = a.rockHeightOverDistance.Evaluate(dist);

			float delay = i * a.delayBetweenRaises;

			var go = MonoBehaviour.Instantiate(a.groundRock, pos, rot);
			go.SetActive(false);
			var particle = MonoBehaviour.Instantiate(a.raisingParticleEffect, pos, rot);

			go.transform.position -= go.transform.up * a.rockHeight;

			//TODO: Run coroutine in proper monobehaviour
			caster.GetComponent<Health>().StartCoroutine(Move(go, particle, delay, a.raiseSpeed, heightAboveGround));
		}

		End();
	}

	IEnumerator Move(GameObject gameObject, GameObject particle, float delay, float raiseSpeed, float raiseHeight)
	{
		yield return new WaitForSeconds(delay);

		float t = 0;
		float extendTime = raiseHeight / raiseSpeed;
		gameObject.SetActive(true);

		particle.GetComponent<ParticleSystem>().Emit(a.createdParticlesOnStartRaise);
		while (t < 1)
		{
			t += Time.deltaTime / extendTime;
			gameObject.transform.position += gameObject.transform.up * Time.deltaTime * raiseSpeed;
			yield return null;
		}
		var e = particle.GetComponent<ParticleSystem>().emission;
		e.enabled = false;
		yield return new WaitForSeconds(a.lifeTime - extendTime * 2);
		e.enabled = true;
		particle.GetComponent<ParticleSystem>().Emit(a.createdParticlesOnStartRaise);
		while (t > 0)
		{
			t -= Time.deltaTime / extendTime;
			gameObject.transform.position -= gameObject.transform.up * Time.deltaTime * raiseSpeed;
			yield return null;
		}


		MonoBehaviour.Destroy(gameObject);
		e.enabled = false;
		MonoBehaviour.Destroy(particle, 2); //Destory after all emmision
	}

	public override void Update()
	{

	}
	public override void OnDrawGizmos()
	{

		Vector3 start = caster.transform.position;
		Vector3 dir = caster.transform.forward;
		Vector3 lastPos = start;
		Vector3 pos;
		Gizmos.color = Color.red;
		float gap = a.range / a.createdRocks;
		for (int i = 0; i < a.createdRocks; i++)
		{
			float dist = gap * i + a.initialRockDistance;
			pos = start + dir * dist;

			if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 1, -1))
			{
				//Make rocks follow ground
				pos = hit.position;
			}



			Gizmos.DrawLine(lastPos, pos);
			Gizmos.DrawWireSphere(pos, 0.05f);
			Gizmos.DrawLine(pos, pos + Vector3.up);

			lastPos = pos;
		}
	}
}

[CreateAssetMenu(menuName = "Game/Spells/Ground Rocks")]
public class GroundRocksSpellAction : SpellAction
{

	[Header("Info")]
	public GameObject groundRock;
	public float range = 5;
	public uint createdRocks = 5;

	public float
		horizontalAngleRange = 10,
		horizontalMovementRange = 0.5f,
	forwardAngleRange = 5,
		 raiseSpeed = 3,
	  	delayBetweenRaises = 0.05f,
	   	lifeTime = 5,
		initialRockDistance = 1.3f,
		rockHeight = 2;


	public AnimationCurve rockHeightOverDistance = AnimationCurve.EaseInOut(0, 1, 5, 2);
	public AnimationCurve rockForwardTiltOverDistance = AnimationCurve.EaseInOut(0, 0, 5, 35);
	[Header("Particles")]
	public GameObject raisingParticleEffect;
	public int createdParticlesOnStartRaise = 5;


	public override Spell BeginCast(Character caster)
	{
		return new GroundRocksSpell(caster, this);
	}
	private void OnValidate()
	{
		//Keep the last node locked at the end, and the start at the start
		range = Mathf.Max(range, 0);
		ClampToRange(rockForwardTiltOverDistance);
		ClampToRange(rockHeightOverDistance);

	}
	void ClampToRange(AnimationCurve c)
	{
		c.keys[0].time = 0;
		var k = c.keys[c.length - 1];
		k.time = range;
		c.MoveKey(c.length - 1, k);
	}
}

