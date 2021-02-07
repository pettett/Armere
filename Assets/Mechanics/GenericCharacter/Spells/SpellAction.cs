using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell
{
	public readonly GameObject caster;

	protected Spell(GameObject caster)
	{
		this.caster = caster;
	}

	public abstract void Cast();
	public abstract void Update();
	public abstract void CancelCast(bool manualCancel);
}



public abstract class SpellAction : ScriptableObject
{
	public abstract Spell BeginCast(GameObject caster);

}
