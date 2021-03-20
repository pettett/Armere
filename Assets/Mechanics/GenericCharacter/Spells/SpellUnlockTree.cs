using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "Game/Progress Trees/Spell Unlock Tree")]
public class SpellUnlockTree : NodeGraph
{
	public SpellAction[] selectedNodes;
}
