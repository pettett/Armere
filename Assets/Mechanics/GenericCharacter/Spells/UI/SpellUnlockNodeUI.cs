using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Armere.UI;
[RequireComponent(typeof(SpellActionUI))]
public class SpellUnlockNodeUI : Selectable, IDragHandler, IEndDragHandler
{
	[HideInInspector] public SpellActionUI node;
	public TextMeshProUGUI textMesh;
	SpellUnlockTreeUI controller;
	public static SpellUnlockNodeUI dragging;

	public static bool isDragging => dragging != null;

	public void Init(SpellAction action)
	{
		node.action = action;
		textMesh.SetText(node.action.name);
		node.thumbnail.sprite = node.action.sprite;
	}
	protected override void Start()
	{
		node = GetComponent<SpellActionUI>();
		controller = GetComponentInParent<SpellUnlockTreeUI>();
	}
	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		BeginDragging();
	}



	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		var input = GetComponentInParent<UIController>().inputReader;

		input.selectSpellEvent += AssignValue;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		var input = GetComponentInParent<UIController>().inputReader;

		input.selectSpellEvent -= AssignValue;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);

		if (!isDragging)
		{

			TooltipUI.current.EndCursorTooltip();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		if (!isDragging)
		{
			ActivateNormalTooptip();
		}
	}
	public void OnEndDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
	}
	public override void OnPointerUp(PointerEventData eventData)
	{
		//If the mouse does not move after begin drag, end drag will not be called, so this is always better?
		base.OnPointerUp(eventData);
		EndDrag(eventData);
	}



	public void AssignValue(InputActionPhase phase, int index)
	{

		if (phase == InputActionPhase.Performed && controller.selections[index].selected != node.action)
		{
			UIController.singleton.autoDragger.AutoDrag(
				node.action.sprite, transform.position, controller.selections[index].transform.position, 1000);
			controller.SetSelection(index, this);
		}
	}


	void BeginDragging()
	{
		dragging = this;
		Armere.UI.TooltipUI.current.OnCursorEnterItemUI(node.action.sprite);
	}
	void ActivateNormalTooptip()
	{
		TooltipUI.current.BeginCursorTooltip(node.action.title, node.action.description);
	}
	public void EndDrag(PointerEventData eventData)
	{

		for (int i = 0; i < eventData.hovered.Count; i++)
		{
			var x = eventData.hovered[i].GetComponent<SpellSelectionUI>();
			if (x != null)
			{
				return;
			}
		}

		if (isDragging)
		{
			dragging = null;
			TooltipUI.current.EndCursorTooltip();
		}
	}


}
