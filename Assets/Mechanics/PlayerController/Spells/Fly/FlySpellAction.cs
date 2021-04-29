using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fly Spell Action", menuName = "Game/Spells/Fly Spell Action", order = 0)]
public class FlySpellAction : SpellAction
{
	[Header("Fly")]
	public float time;

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
		Debug.Log("Starting fly");
		caster.StartCoroutine(Fly());
	}
	public IEnumerator Fly()
	{
		float remaining = action.time;

		action.enableTimer.RaiseEvent(true);
		while (remaining > 0)
		{
			action.setTimerValue.RaiseEvent(remaining, action.time);
			remaining -= Time.deltaTime;
			yield return null;
		}

		action.enableTimer.RaiseEvent(false);

		((Armere.PlayerController.Walking)((Armere.PlayerController.PlayerController)caster).currentState).CancelSpellCast(true);


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