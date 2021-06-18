using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Armere.PlayerController
{
	public class RockThrowSpell : Spell
	{
		readonly Projectile createdRock;
		readonly RockThrowSpellAction a;
		Vector3 vel;
		protected override UIKeyPromptGroup.KeyPrompt[] KeyPrompts => new UIKeyPromptGroup.KeyPrompt[]
		{
			("Raise Barrier", InputReader.GroundActionMapActions.Attack),
		};
		public RockThrowSpell(Walking caster, RockThrowSpellAction a) : base(caster)
		{
			this.a = a;
			//Make the rock come from the bottom
			createdRock = MonoBehaviour.Instantiate(a.rockPrefab, GetPoint() + Vector3.down * 3, Quaternion.identity);

		}
		Vector3 GetPoint()
		{
			return caster.transform.TransformPoint(new Vector3(
				Oscillate(1, 0.1f, 0.9f, 0),
				Oscillate(1.5f, 0.1f, 1f, 1.5f),
				Oscillate(1, 0.1f, 0.9f, 3)
			));
		}
		public static float Oscillate(float mean, float intensity, float frequency, float offset)
		{
			return Mathf.Sin(Time.time * frequency + offset) * intensity + mean;
		}
		public override void Update()
		{
			createdRock.transform.position = Vector3.SmoothDamp(
				createdRock.transform.position,
				GetPoint(),
				ref vel, 0.1f, 100);
		}

		public override void EndCast(bool manualCancel)
		{
			MonoBehaviour.Destroy(createdRock.gameObject);

			ClearControlBindings();
			a.input.attackEvent -= Cast;
		}

		public void Cast(InputActionPhase phase)
		{
			createdRock.LaunchProjectile(caster.transform.forward * 10);

			caster.CancelSpellCast(true);
		}

		public override void Begin()
		{
			AddControlBindings(a.input);
			a.input.attackEvent += Cast;
		}
	}

	[CreateAssetMenu(menuName = "Game/Spells/Rock Throw")]
	public class RockThrowSpellAction : SpellAction
	{
		public Projectile rockPrefab;
		public override Spell BeginCast(Walking caster)
		{
			return new RockThrowSpell(caster, this);
		}

	}
}