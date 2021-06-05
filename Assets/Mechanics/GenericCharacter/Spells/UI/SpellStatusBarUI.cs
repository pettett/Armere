using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellStatusBarUI : MonoBehaviour
{
	public SpellUnlockTree tree;
	public GameObject selectionPrefab;
	[System.NonSerialized] public SpellStatusUI[] selections;
	public Transform selectionsBar;
	// Start is called before the first frame update
	void Start()
	{
		selections = new SpellStatusUI[tree.selectedNodes.Length];
		for (int i = 0; i < tree.selectedNodes.Length; i++)
		{
			selections[i] = Instantiate(selectionPrefab, selectionsBar).GetComponent<SpellStatusUI>();
			selections[i].SetStatus(tree.selectedNodes[i]);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
