using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProgressNode<NodeT> : ScriptableObject where NodeT : ProgressNode<NodeT>
{
	public NodeT[] dependencies;

	public int Tier()
	{
		int maxTier = -1;
		if (dependencies != null)
			for (int i = 0; i < dependencies.Length; i++)
			{
				maxTier = Mathf.Max(dependencies[i].Tier());
			}
		return maxTier + 1;
	}

}
