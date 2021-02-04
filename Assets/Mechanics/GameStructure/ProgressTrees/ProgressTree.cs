using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Progress Trees/Progress Tree")]
public abstract class ProgressTree<NodeT> : ScriptableObject where NodeT : ProgressNode<NodeT>
{
	public NodeT[] nodes;
}
