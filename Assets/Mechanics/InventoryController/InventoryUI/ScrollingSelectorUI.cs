using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

namespace Armere.Inventory.UI
{

	public class ScrollingSelectorUI : MonoBehaviour
	{

		public InventoryController selectingInventory;

		[System.Serializable]
		public struct SelectionLayer
		{
			[System.NonSerialized] public ItemType selecting;
			[System.NonSerialized] public int selection;
			public RectTransform childGroup;
			[System.NonSerialized] public InventoryPanel panel;
			[System.NonSerialized] public float internalScroll, scrollVel, scroll; //Scroll in units of selection
		}
		public SelectionLayer[] layers = new SelectionLayer[2];


		public Vector2 optionSize;
		public float optionSpacing = 10;
		public float scrollSensitivity = 1 / 240f;
		public RectTransform optionDotsGroup;
		public GameObject itemDisplayPrefab;
		public GameObject blankPanelPrefab;
		public TextMeshProUGUI selectedTitle;
		public float layerSeperation = 50;
		int layer;
		public InputReader input;

		private void OnEnable()
		{

			for (int i = 0; i < layers.Length; i++)
			{
				layers[i].childGroup.GetComponent<HorizontalLayoutGroup>().spacing = optionSpacing;

				layers[i].panel = selectingInventory.GetPanelFor(layers[i].selecting);

				//Create the first, blank panel
				GameObject child = Instantiate(blankPanelPrefab, layers[i].childGroup);
				var l = child.AddComponent<LayoutElement>();
				l.preferredHeight = optionSize.y;
				l.preferredWidth = optionSize.y;

				//Create all the option panels
				for (int ii = 0; ii < layers[i].panel.stackCount; ii++)
				{
					child = Instantiate(itemDisplayPrefab, layers[i].childGroup);

					l = child.AddComponent<LayoutElement>();

					l.preferredHeight = optionSize.y;
					l.preferredWidth = optionSize.y;

					InventoryItemUI itemDisplay = child.GetComponent<InventoryItemUI>();
					itemDisplay.type = layers[i].panel[ii].item.type;
					itemDisplay.ChangeItemIndex(ii);
				}

				layers[i].internalScroll = layers[i].selection + 1;
				layers[i].scroll = layers[i].selection;
				layers[i].scrollVel = 0;
				SetTitleText();

				layers[i].childGroup.sizeDelta = Vector2.right * ((layers[i].panel.stackCount + 1) * optionSize.x + optionSpacing * layers[i].panel.stackCount);
				UpdateChildGroup(i);


			}

			input.SwitchToUIInput();

			input.uiScrollEvent += OnScroll;

			input.uiNavigateEvent += OnMovement;
		}

		private void OnDisable()
		{
			foreach (var l in layers) foreach (Transform t in l.childGroup) Destroy(t.gameObject);

			input.uiScrollEvent -= OnScroll;
			input.uiNavigateEvent -= OnMovement;
			input.SwitchToGameplayInput();
		}

		void OnScroll(InputAction.CallbackContext value)
		{
			if (value.phase == InputActionPhase.Performed)
			{
				float d = value.ReadValue<Vector2>().y;

				//Move the selection by the direction of input
				layers[layer].internalScroll = Mathf.Clamp(layers[layer].internalScroll + Mathf.Sign(d), 0, layers[layer].panel.stackCount);
				//Allow for un equipping items
				layers[layer].selection = Mathf.RoundToInt(layers[layer].internalScroll) - 1;
				SetTitleText();
			}
		}
		public void OnMovement(Vector2 direction)
		{
			if (direction.y > 0.5f)
				SetLayer(Mathf.Min(layer + 1, layers.Length - 1));
			if (direction.y < -0.5f)
				SetLayer(Mathf.Max(layer - 1, 0));
		}
		public void SetLayer(int newLayer)
		{
			layers[layer].childGroup.GetComponent<Image>().enabled = false;
			layer = newLayer;
			layers[layer].childGroup.GetComponent<Image>().enabled = true;
		}
		void SetTitleText()
		{
			if (layers[layer].selection != -1)
			{
				selectedTitle.text = layers[layer].panel[layers[layer].selection].item.displayName;
				selectedTitle.enabled = true;
			}
			else
			{
				selectedTitle.enabled = false;
			}
		}
		private void Update()
		{
			//This must script work while game is paused
			layers[layer].scroll = Mathf.SmoothDamp(layers[layer].scroll, layers[layer].selection, ref layers[layer].scrollVel, 0.1f, 10, Time.unscaledDeltaTime);

			UpdateChildGroup(layer);
		}
		void UpdateChildGroup(int index)
		{
			layers[index].childGroup.anchoredPosition = Vector2.up * index * layerSeperation - Vector2.right * (layers[index].scroll * (optionSize.x + optionSpacing) - layers[index].childGroup.sizeDelta.x * 0.5f + optionSize.x * 1.5f + optionSpacing);
		}


	}
}