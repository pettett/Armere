using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Armere.PlayerController
{
	[CreateAssetMenu(fileName = "Fly Spell Action", menuName = "Game/Spells/Fly Spell Action", order = 0)]
	public class FlySpellAction : SpellAction
	{
		[Header("Fly")]
		public float time;
		public MovementStateTemplate flying;

		[Header("Timer")]
		public BoolEventChannelSO enableTimer;
		public FloatFloatEventChannelSO setTimerValue;
		public override Spell BeginCast(Walking caster)
		{
			return new FlySpell(caster, this);
		}
	}

	public class FlySpell : Spell
	{
		readonly FlySpellAction action;
		Coroutine coroutine;
		public FlySpell(Walking caster, FlySpellAction action) : base(caster)
		{
			this.action = action;
		}

		public override void Begin()
		{
			coroutine = caster.c.StartCoroutine(Fly());
		}
		void Cancel()
		{
			caster.c.StopCoroutine(coroutine);
			End();

		}
		public IEnumerator Fly()
		{
			Debug.Log("Starting fly");
			float remaining = action.time;

			action.enableTimer.RaiseEvent(true);


			caster.c.ChangeToStateTimed(action.flying, action.time);

			caster.c.onStateChanged += Cancel;
			while (remaining > 0)
			{
				action.setTimerValue.RaiseEvent(remaining, action.time);
				remaining -= Time.deltaTime;
				yield return null;
			}

			End();



			Debug.Log("Ending fly");
		}
		void End()
		{

			action.enableTimer.RaiseEvent(false);
			caster.c.onStateChanged -= Cancel;
		}

		public override void EndCast(bool manualCancel)
		{
		}


		public override void Update()
		{
		}
	}
}