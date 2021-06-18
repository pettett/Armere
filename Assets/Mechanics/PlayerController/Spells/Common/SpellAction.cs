using System.Collections;
using System.Collections.Generic;
using Armere.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using XNode;
namespace Armere.PlayerController
{
	public abstract class Spell
	{
		public readonly Walking caster;

		public virtual bool CanAttackWhileInUse => true;
		public virtual bool CancelOnAltAttack => true;

		protected Spell(Walking caster)
		{
			this.caster = caster;
		}
		public abstract void Begin();
		public abstract void Update();
		public abstract void EndCast(bool manualCancel);
		public virtual void OnDrawGizmos() { }

		protected virtual UIKeyPromptGroup.KeyPrompt[] KeyPrompts => new UIKeyPromptGroup.KeyPrompt[0];

		/// <summary>
		/// Adds binding to input reader and shows prompt ui for it
		/// </summary>
		protected void AddControlBindings(InputReader input)
		{
			var prompt = KeyPrompts;
			if (CancelOnAltAttack)
			{
				UIKeyPromptGroup.KeyPrompt[] p = new UIKeyPromptGroup.KeyPrompt[prompt.Length + 1];
				System.Array.Copy(prompt, p, prompt.Length);
				p[prompt.Length] = ("Cancel", InputReader.GroundActionMapActions.AltAttack);
				prompt = p;
			}

			UIKeyPromptGroup.singleton.ShowPrompts(input, InputReader.groundActionMap, prompt);
		}

		protected void ClearControlBindings()
		{
			UIKeyPromptGroup.singleton.RemovePrompts();

		}

		public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => MonoBehaviour.Instantiate(original, position, rotation);
		public bool GetWorldTargetPos(LayerMask layerMask, out Vector3 position, float minDistance, float maxDistance)
		{
			Vector3 target;
			if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxDistance * 4, layerMask, QueryTriggerInteraction.Ignore))
			{
				//Limit distance
				Vector3 dir = hit.point - caster.transform.position;
				float m = dir.magnitude;
				dir /= m; //Dir now magnitude 1
				m = Mathf.Clamp(m, minDistance, maxDistance);
				dir *= m;
				target = caster.transform.position + dir;
			}
			else
			{
				target = caster.transform.position + caster.transform.forward * maxDistance;
			}

			if (NavMesh.Raycast(caster.transform.position, target, out NavMeshHit navMeshHit, NavMesh.AllAreas))
			{
				//Hit gap in navmesh between player and target
				position = navMeshHit.position;
				return true;
			}
			else if (NavMesh.SamplePosition(target, out navMeshHit, 1, NavMesh.AllAreas))
			{
				position = navMeshHit.position;
				return true;
			}
			else
			{
				position = default;
				return false;
			}

		}
	}



	public abstract class SpellAction : Node
	{
		public float rechargeTime = 1f;
		[Header("Prompts")]
		public InputReader input;
		[Header("Info")]
		public string title;
		[TextArea] public string description;
		public Sprite sprite;





		[Input] public SpellAction dependency;
		[Output] public SpellAction self;

		public override object GetValue(NodePort port)
		{
			self = this;
			return this;
		}

		public abstract Spell BeginCast(Walking caster);

	}
}