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
    private void Start()
    {
        leftButton.onClick.AddListener(Left);
        rightButton.onClick.AddListener(Right);
        for (int i = 0; i < panelHolder.childCount; i++)
        {
            panelHolder.GetChild(i).gameObject.SetActive(i == openPanel);
        }
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
