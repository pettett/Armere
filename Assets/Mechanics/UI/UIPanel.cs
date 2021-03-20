using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel : MonoBehaviour
{
	public UIPanels parent;

	public Button openButton;


	private void Awake()
	{
		openButton.onClick.AddListener(() => parent.SetPanel(this));
	}

	public void MakeSelected()
	{
		openButton.colors = parent.buttonSelected;

	}
	public void MakeUnSelected()
	{
		openButton.colors = parent.buttonNormal;
	}

}
