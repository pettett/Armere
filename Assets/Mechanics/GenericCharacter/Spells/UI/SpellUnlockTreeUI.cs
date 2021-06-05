using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SpellUnlockTreeUI : MonoBehaviour
{
	public SpellUnlockTree tree;
	public Transform unlockNodesTree;
	public Transform selectionsBar;
	public GameObject nodePrefab;
	public GameObject selectionPrefab;

	public Vector2 positionScale = new Vector2(1, 0.2f);

	[System.NonSerialized] public SpellSelectionUI[] selections;

	public void SetSelection(int index, SpellUnlockNodeUI node)
	{
		if (tree.selectedNodes[index].canBeReplaced)
		{
			tree.selectedNodes[index].ReplaceActionWith(node.node.action);
			selections[index].SetSelection(node.node.action);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		Vector2 topRight = Vector2.negativeInfinity;

		Vector2 bottomLeft = Vector2.positiveInfinity;

		//Instantiate copies of spell nodes and selection boxes to the ui canvas
		for (int i = 0; i < tree.nodes.Count; i++)
		{
			Vector2 pos = Vector2.Scale(tree.nodes[i].position, positionScale);

			var node = Instantiate(nodePrefab, unlockNodesTree, false);
			((RectTransform)node.transform).anchorMin = Vector2.zero;
			((RectTransform)node.transform).anchorMax = Vector2.zero;
			((RectTransform)node.transform).anchoredPosition = pos;

			topRight = Vector2.Max(topRight, pos);
			bottomLeft = Vector2.Min(bottomLeft, pos);

			node.GetComponent<SpellUnlockNodeUI>().node.SetAction((SpellAction)tree.nodes[i]);
		}

		bottomLeft -= new Vector2(100, 200);
		topRight += new Vector2(100, 200);

		foreach (RectTransform child in unlockNodesTree)
		{
			child.anchoredPosition -= bottomLeft;
		}

		((RectTransform)unlockNodesTree).sizeDelta = topRight - bottomLeft;


		selections = new SpellSelectionUI[tree.selectedNodes.Length];
		for (int i = 0; i < tree.selectedNodes.Length; i++)
		{
			selections[i] = Instantiate(selectionPrefab, selectionsBar).GetComponent<SpellSelectionUI>();
			selections[i].action.SetAction(tree.selectedNodes[i].spell);
			selections[i].index = i;
			selections[i].SetSelection(tree.selectedNodes[i].spell);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

}
