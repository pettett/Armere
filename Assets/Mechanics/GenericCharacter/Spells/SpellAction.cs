using System.Collections;
using System.Collections.Generic;
using Armere.UI;
using UnityEngine;
using UnityEngine.AI;
using XNode;

public enum CastType
{
	PrimaryFire,
	ReleaseCharge,
	None
}
public abstract class Spell
{
	public readonly Character caster;
	public readonly CastType castType;

	protected Spell(Character caster, CastType castType = CastType.PrimaryFire)
	{
		this.caster = caster;
		this.castType = castType;
	}
	public abstract void Begin();
	public abstract void Cast();
	public abstract void Update();
	public abstract void CancelCast(bool manualCancel);
	public virtual void OnDrawGizmos() { }
	protected void HidePrompts()
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

	public abstract Spell BeginCast(Character caster);

}
