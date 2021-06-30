using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Armere.PlayerController.UI
{
	[RequireComponent(typeof(SpellActionUI))]
	public class SpellSelectionUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[System.NonSerialized] public int index;

		[System.NonSerialized] public SpellAction selected;
		[System.NonSerialized] public SpellActionUI action;

		private void Awake()
		{
			action = GetComponent<SpellActionUI>();
		}

		public void OnDrop(PointerEventData eventData)
		{
			Debug.Log("Dropping on selection");
			GetComponentInParent<SpellUnlockTreeUI>().SetSelection(index, SpellUnlockNodeUI.dragging);

			Armere.UI.TooltipUI.current.EndCursorTooltip();
			LeanTween.scale(action.thumbnail.gameObject, Vector3.one, 0.05f).setIgnoreTimeScale(true);

			SpellUnlockNodeUI.dragging = null;
		}

		public void SetSelection(SpellAction selection)
		{
			if (selection != null)
			{

				//TODO: Check if unlocked
				action.thumbnail.sprite = selection.sprite;

				selected = selection;

			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (SpellUnlockNodeUI.dragging != null && selected != SpellUnlockNodeUI.dragging.node)
			{
				LeanTween.scale(action.thumbnail.gameObject, Vector3.one * 1.1f, 0.05f).setIgnoreTimeScale(true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (SpellUnlockNodeUI.dragging != null && selected != SpellUnlockNodeUI.dragging.node)
			{

				LeanTween.scale(action.thumbnail.gameObject, Vector3.one, 0.05f).setIgnoreTimeScale(true);
			}
		}
	}
}