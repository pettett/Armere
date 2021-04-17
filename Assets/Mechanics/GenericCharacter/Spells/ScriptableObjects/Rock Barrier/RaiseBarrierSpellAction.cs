using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RaiseBarrierSpell : Spell
{

	public readonly RaiseBarrierSpellAction a;

	GameObject preview;

	public RaiseBarrierSpell(Character caster, RaiseBarrierSpellAction action) : base(caster)
	{
		a = action;


		preview = Instantiate(a.previewBarrierRock, Vector3.zero, Quaternion.identity);
	}

	void End()
	{
		MonoBehaviour.Destroy(preview);
		HidePrompts();
	}

	public override void CancelCast(bool manualCancel)
	{
		End();
	}

	public override void Cast()
	{
		if (GetWorldTargetPos(a.groundLayerMask, out Vector3 target, a.minRange, a.maxRange))
		{
			Instantiate(a.barrierRock, target, caster.transform.rotation);
		}

		End();
	}

	IEnumerator Move(GameObject gameObject, GameObject particle, float delay, float raiseSpeed, float raiseHeight)
	{
		yield return new WaitForSeconds(delay);

		float t = 0;
		float extendTime = raiseHeight / raiseSpeed;
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
		if (GetWorldTargetPos(a.groundLayerMask, out Vector3 target, a.minRange, a.maxRange))
		{
			preview.transform.position = target;
			preview.transform.rotation = caster.transform.rotation;
			if (preview.activeInHierarchy == false)
			{
				preview.SetActive(true);
			}
		}
		else if (preview.activeInHierarchy == true)
		{
			preview.SetActive(false);
		}
	}
	public override void OnDrawGizmos()
	{
		if (GetWorldTargetPos(a.groundLayerMask, out Vector3 target, a.minRange, a.maxRange))
			Gizmos.DrawWireSphere(target, 0.2f);
	}

	public override void Begin()
	{
		UIKeyPromptGroup.singleton.ShowPrompts(a.input, InputReader.groundActionMap, ("Raise Barrier", InputReader.attackAction));
	}
}

[CreateAssetMenu(menuName = "Game/Spells/Raise Barrier")]
public class RaiseBarrierSpellAction : SpellAction
{

	[Header("Info")]
	public GameObject previewBarrierRock;
	public GameObject barrierRock;
	public LayerMask groundLayerMask;
	public float minRange = 1, maxRange = 5, lifeTime = 20;


	[Header("Particles")]
	public GameObject raisingParticleEffect;
	public int createdParticlesOnStartRaise = 5;


	public override Spell BeginCast(Character caster)
	{
		return new RaiseBarrierSpell(caster, this);
	}

}

