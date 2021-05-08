using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.PlayerController;

[CreateAssetMenu(fileName = "Fly Spell Action", menuName = "Game/Spells/Fly Spell Action", order = 0)]
public class FlySpellAction : SpellAction
{
	[Header("Fly")]
	public float time;
	public MovementStateTemplate flying;

	[Header("Timer")]
	public BoolEventChannelSO enableTimer;
	public FloatFloatEventChannelSO setTimerValue;
	public override Spell BeginCast(Character caster)
	{
		return new FlySpell(caster, this);
	}
}

public class FlySpell : Spell
{
	readonly FlySpellAction action;
	public FlySpell(Character caster, FlySpellAction action) : base(caster, CastType.None)
	{
		this.action = action;
	}

	public override void Begin()
	{
		caster.StartCoroutine(Fly());
	}
	public IEnumerator Fly()
	{
		Debug.Log("Starting fly");
		float remaining = action.time;

		action.enableTimer.RaiseEvent(true);


		((PlayerController)caster).ChangeToStateTimed(action.flying, action.time);

		while (remaining > 0)
		{
			action.setTimerValue.RaiseEvent(remaining, action.time);
			remaining -= Time.deltaTime;
			yield return null;
		}

		action.enableTimer.RaiseEvent(false);



		Debug.Log("Ending fly");
	}

	public override void CancelCast(bool manualCancel)
	{
	}

	public override void Cast()
	{
	}

	public override void Update()
	{
	}
}