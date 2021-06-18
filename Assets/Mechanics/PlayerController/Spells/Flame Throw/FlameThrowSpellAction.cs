using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Armere.PlayerController
{
	public class FlameThrowSpellAction : SpellAction
	{
		[Header("Timer")]
		public BoolEventChannelSO enableTimer;
		public FloatFloatEventChannelSO setTimerValue;
		[Header("Fire")]
		public float flameTime = 5f;
		public FlammableBody fireCaster;
		public override Spell BeginCast(Walking caster)
		{
			return new FlameThrowSpell(caster, this);
		}
	}
	public class FlameThrowSpell : Spell
	{
		readonly FlameThrowSpellAction a;
		float remianingTime;
		bool flaming = false;
		public override bool CanAttackWhileInUse => false;
		FlammableBody fireSpreader;


		protected override UIKeyPromptGroup.KeyPrompt[] KeyPrompts => new UIKeyPromptGroup.KeyPrompt[]
		{
			("Fire", InputReader.GroundActionMapActions.Attack),
		};
		public FlameThrowSpell(Walking caster, FlameThrowSpellAction a) : base(caster)
		{
			this.a = a;
			remianingTime = a.flameTime;
		}

		public override void Begin()
		{
			a.enableTimer?.RaiseEvent(true);
			fireSpreader = caster.c.weaponGraphics.InstantiateHeldObject(a.fireCaster);

			AddControlBindings(a.input);
			a.input.attackEvent += OnAttackEvent;
		}

		public override void EndCast(bool manualCancel)
		{
			ClearControlBindings();
			a.input.attackEvent -= OnAttackEvent;
			a.enableTimer?.RaiseEvent(false);
			caster.c.weaponGraphics.DestroyHeldObject(fireSpreader.gameObject);
		}


		public void OnAttackEvent(InputActionPhase phase)
		{
			if (phase == InputActionPhase.Performed)
			{
				flaming = true;
				fireSpreader.Light();
			}
			else if (phase == InputActionPhase.Canceled)
			{

				flaming = false;
				fireSpreader.Extinguish();
			}
		}

		public override void Update()
		{
			if (flaming)
			{
				remianingTime -= Time.deltaTime;
				a.setTimerValue?.RaiseEvent(remianingTime, a.flameTime);
				if (remianingTime <= 0)
				{
					//end the spell
					caster.CancelSpellCast(true);
				}
			}
		}
	}
}