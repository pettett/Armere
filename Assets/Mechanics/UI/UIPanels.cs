using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIPanels : MonoBehaviour
{
	public RectTransform panelHolder;
	public Button leftButton;
	public Button rightButton;
	public RectTransform currentPanelDisplay;
	public int openPanel;
	public IntEventChannelSO setOpenPanelEventChannel;

	private void Start()
	{
		leftButton.onClick.AddListener(Left);
		rightButton.onClick.AddListener(Right);
		for (int i = 0; i < panelHolder.childCount; i++)
		{
			panelHolder.GetChild(i).gameObject.SetActive(i == openPanel);
		}
		if (setOpenPanelEventChannel != null)
			setOpenPanelEventChannel.OnEventRaised += SetPanel;
	}
	private void OnDestroy()
	{
		if (setOpenPanelEventChannel != null)
			setOpenPanelEventChannel.OnEventRaised -= SetPanel;
	}


	void Left()
	{
		int prevPanel = openPanel;
		openPanel = Mathf.Clamp(openPanel + 1, 0, panelHolder.childCount - 1);
		if (prevPanel != openPanel) SwitchPanels(prevPanel, openPanel);
	}
	void Right()
	{
		int prevPanel = openPanel;
		openPanel = Mathf.Clamp(openPanel - 1, 0, panelHolder.childCount - 1);
		if (prevPanel != openPanel) SwitchPanels(prevPanel, openPanel);
	}
	void SetPanel(int panel)
	{
		if (panel != openPanel)
		{
			SwitchPanels(openPanel, panel);
			openPanel = panel;
		}
	}
	void SwitchPanels(int from, int to)
	{
		StartCoroutine(AnimatePanels(from, to));

	}
	IEnumerator AnimatePanels(int from, int to)
	{

		panelHolder.GetChild(to).gameObject.SetActive(true);
		yield return null;
		panelHolder.GetChild(from).gameObject.SetActive(false);
	}
}
