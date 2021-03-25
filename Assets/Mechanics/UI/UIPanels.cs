using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIPanels : MonoBehaviour
{
	public InputReader input;
	public UIPanel[] panels;
	public RectTransform currentPanelDisplay;
	public int openPanel;
	public IntEventChannelSO setOpenPanelEventChannel;
	public float transitionTime = 0.1f;
	float endTransition;
	[Header("Button selected")]
	public ColorBlock buttonSelected;
	[Header("Button normal")]
	public ColorBlock buttonNormal;
	private void Start()
	{
		input.SwitchToUIInput();

		SetOpenPanel();
		if (setOpenPanelEventChannel != null)
			setOpenPanelEventChannel.OnEventRaised += SetPanel;
	}
	private void OnEnable()
	{

		input.uiNavigateHorizontalEvent += NavigateHorizontal;
	}
	private void OnDisable()
	{

		input.uiNavigateHorizontalEvent -= NavigateHorizontal;
	}

	private void OnDestroy()
	{
		if (setOpenPanelEventChannel != null)
			setOpenPanelEventChannel.OnEventRaised -= SetPanel;
	}

	void NavigateHorizontal(InputActionPhase context, float horizontal)
	{
		if (context == InputActionPhase.Performed && Time.time > endTransition)
		{
			if (horizontal > 0)
			{
				Left();
			}
			else
			{
				Right();
			}
		}
	}


	private void OnValidate()
	{
		openPanel = Mathf.Clamp(openPanel, 0, panels.Length - 1);
		SetOpenPanel();
	}

	void SetOpenPanel()
	{
		for (int i = 0; i < panels.Length; i++)
		{
			panels[i].gameObject.SetActive(openPanel == i);
			if (openPanel == i)
			{
				panels[i].MakeSelected();
			}
			else
			{
				panels[i].MakeUnSelected();
			}
		}
	}


	void Left()
	{
		int prevPanel = openPanel;
		openPanel = Mathf.Clamp(openPanel + 1, 0, panels.Length - 1);
		if (prevPanel != openPanel) SwitchPanels(prevPanel, openPanel);
	}
	void Right()
	{
		int prevPanel = openPanel;
		openPanel = Mathf.Clamp(openPanel - 1, 0, panels.Length - 1);
		if (prevPanel != openPanel) SwitchPanels(prevPanel, openPanel);
	}
	public void SetPanel(UIPanel panel)
	{
		int i = System.Array.FindIndex(panels, x => x == panel);
		SetPanel(i);
	}
	public void SetPanel(int panel)
	{

		if (panel != openPanel)
		{
			SwitchPanels(openPanel, panel);
			openPanel = panel;
		}
	}
	void SwitchPanels(int from, int to)
	{
		panels[to].MakeSelected();
		panels[from].MakeUnSelected();
		panels[to].openButton.Select();

		float direction = Mathf.Sign(to - from);

		panels[to].transform.localPosition = new Vector2(2000 * direction, 0);
		endTransition = Time.time + transitionTime;
		panels[to].gameObject.SetActive(true);

		LeanTween.moveLocal(panels[to].gameObject, Vector2.zero, transitionTime).setIgnoreTimeScale(true).setEaseInCubic().setEaseOutCubic();

		LeanTween.moveLocal(panels[from].gameObject, new Vector2(-2000 * direction, 0), transitionTime).setIgnoreTimeScale(true).setEaseInCubic(
		).setEaseOutCubic().setOnComplete(() =>
		{
			panels[from].gameObject.SetActive(false);
		});



	}
}
