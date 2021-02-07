using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Progress Trees/Spell Unlock Node")]
public class SpellUnlockNode : ProgressNode<SpellUnlockNode>
{
	public SpellAction spell;
}
