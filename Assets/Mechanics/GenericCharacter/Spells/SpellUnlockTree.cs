using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Progress Trees/Spell Unlock Tree")]
public class SpellUnlockTree : ProgressTree<SpellUnlockNode>
{
	public SpellUnlockNode[] selectedNodes;
}
