using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SpellUnlockTreeUI : MonoBehaviour
{
	public SpellUnlockTree tree;
	public GameObject nodePrefab;

	public SpellSelectionUI[] selections;

	public class UINode
	{
		public float x;
		public float y;
		public SpellAction action;

		public UINode parent;

		public List<UINode> children;
		public List<UINode> siblings;

	}

	public UINode[] nodes;

	public void SetSelection(int index, SpellUnlockNodeUI node)
	{
		selections[index].SetSelection(node.node);
		tree.selectedNodes[index] = node.node;

	}

	// Start is called before the first frame update
	void Start()
	{
		nodes = new UINode[tree.nodes.Count];
		for (int i = 0; i < tree.nodes.Count; i++)
		{
			nodes[i] = new UINode();
			nodes[i].action = (SpellAction)tree.nodes[i];
			nodes[i].siblings = new List<UINode>();
			nodes[i].children = new List<UINode>();

		}
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].action.dependency != null)
			{
				for (int ii = 0; ii < nodes.Length; ii++)
				{
					if (nodes[ii].action == nodes[i].action.dependency)
					{
						nodes[ii].children.Add(nodes[i]);
						nodes[i].parent = nodes[ii];
						break;
					}
				}
			}
		}
		for (int i = 0; i < nodes.Length; i++)
		{
			for (int j = 0; j < nodes[i].children.Count; j++)
			{
				for (int jj = 0; jj < nodes[i].children.Count; jj++)
				{
					if (j != jj)
					{
						nodes[i].children[j].siblings.Add(nodes[i].children[jj]);
					}
				}
			}
		}
		for (int i = 0; i < selections.Length; i++)
		{
			selections[i].index = i;
			selections[i].SetSelection(tree.selectedNodes[i]);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

}
