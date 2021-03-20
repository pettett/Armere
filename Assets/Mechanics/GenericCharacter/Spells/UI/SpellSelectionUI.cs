using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellSelectionUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	Image image;
	[System.NonSerialized] public int index;

	SpellAction selected;

	private void Awake()
	{
		image = GetComponentInChildren<Image>();
	}

	public void OnDrop(PointerEventData eventData)
	{
		Debug.Log("Dropping on selection");
		GetComponentInParent<SpellUnlockTreeUI>().SetSelection(index, SpellUnlockNodeUI.dragging);

		Armere.UI.TooltipUI.current.EndCursorTooltip();
		LeanTween.scale(image.gameObject, Vector3.one, 0.05f);

		SpellUnlockNodeUI.dragging = null;
	}

	public void SetSelection(SpellAction selection)
	{
		if (selection != null)
		{

			//TODO: Check if unlocked
			image.sprite = selection.sprite;

			selected = selection;

		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (SpellUnlockNodeUI.dragging != null && selected != SpellUnlockNodeUI.dragging.node)
		{
			LeanTween.scale(image.gameObject, Vector3.one * 1.1f, 0.05f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (SpellUnlockNodeUI.dragging != null && selected != SpellUnlockNodeUI.dragging.node)
		{

			LeanTween.scale(image.gameObject, Vector3.one, 0.05f);
		}
	}
}
