using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
[System.Serializable]
public class SpellStatus
{
	public SpellAction spell { get; private set; }
	[System.NonSerialized] public float timeToUse;
	public bool canBeUsed => spell != null && canBeReplaced;
	public bool canBeReplaced => timeToUse < Time.time;
	public event System.Action onSpellChanged;

	public SpellAction Use()
	{
		timeToUse = Time.time + spell.rechargeTime;
		return spell;
	}
	public void ReplaceActionWith(SpellAction newSpell)
	{
		if (canBeReplaced)
		{
			spell = newSpell;
			timeToUse = 0;
			onSpellChanged?.Invoke();
		}
	}
}

[CreateAssetMenu(menuName = "Game/Progress Trees/Spell Unlock Tree")]
public class SpellUnlockTree : NodeGraph
{
	public SpellStatus[] selectedNodes = new SpellStatus[5];
}
