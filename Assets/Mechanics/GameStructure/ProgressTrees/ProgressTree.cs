using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Progress Trees/Progress Tree")]
public abstract class ProgressTree<NodeT> : SaveableSO where NodeT : ProgressNode<NodeT>
{
	public NodeT[] nodes;

	public override void SaveBin(in GameDataWriter writer)
	{

	}
	public override void LoadBin(in GameDataReader reader)
	{

	}
	public override void LoadBlank()
	{

	}
}
