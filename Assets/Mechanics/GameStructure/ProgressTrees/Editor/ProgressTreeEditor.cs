using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class ProgressTreeEditor<TreeT, NodeT> : Editor
	where NodeT : ProgressNode<NodeT>
	where TreeT : ProgressTree<NodeT>
{

	TreeT t => (TreeT)target;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		List<List<NodeT>> nodeTiers = new List<List<NodeT>>();
		//Tier each node
		for (int i = 0; i < t.nodes.Length; i++)
		{
			int tier = t.nodes[i].Tier();
			while (tier >= nodeTiers.Count)
			{
				nodeTiers.Add(new List<NodeT>());
			}
			nodeTiers[tier].Add(t.nodes[i]);
		}
		//Display them

		for (int i = 0; i < nodeTiers.Count; i++)
		{
			if (nodeTiers[i].Count != 0)
			{
				GUILayout.Label($"Tier {i}");
				for (int ii = 0; ii < nodeTiers[i].Count; ii++)
				{
					GUILayout.Label(nodeTiers[i][ii].name);
				}
			}
		}

	}
}
