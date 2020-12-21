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

        public InputAction scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll");
        public ItemType selectingType;
        public int selection;
        float scrollVel = 0;
        public float scroll; //Scroll in units of selection
        public Vector2 optionSize;
        public float optionSpacing = 10;
        public float scrollSensitivity = 1 / 240f;
        public RectTransform childGroup;
        public RectTransform optionDotsGroup;
        InventoryPanel panel;
        public GameObject itemDisplayPrefab;
        public GameObject blankPanelPrefab;
        float internalScroll = 0;
        public TextMeshProUGUI selectedTitle;
        private void OnEnable()
        {

            childGroup.GetComponent<HorizontalLayoutGroup>().spacing = optionSpacing;


            panel = InventoryController.singleton.GetPanelFor(selectingType);

            //Create the first, blank panel
            GameObject child = Instantiate(blankPanelPrefab, childGroup);
            var l = child.AddComponent<LayoutElement>();
            l.preferredHeight = optionSize.y;
            l.preferredWidth = optionSize.y;

            //Create all the option panels
            for (int i = 0; i < panel.stackCount; i++)
            {
                child = Instantiate(itemDisplayPrefab, childGroup);

                l = child.AddComponent<LayoutElement>();

                l.preferredHeight = optionSize.y;
                l.preferredWidth = optionSize.y;

                InventoryItemUI itemDisplay = child.GetComponent<InventoryItemUI>();
                itemDisplay.type = InventoryController.singleton.db[panel[i].name].type;
                itemDisplay.ChangeItemIndex(i);
            }
            internalScroll = selection + 1;
            scroll = selection;
            scrollVel = 0;
            SetTitleText();

            childGroup.sizeDelta = Vector2.right * ((panel.stackCount + 1) * optionSize.x + optionSpacing * panel.stackCount);
            UpdateChildGroup();


            scrollAction.performed += OnScroll;
            scrollAction.Enable();
        }
        private void OnDisable()
        {
            foreach (Transform t in childGroup) Destroy(t.gameObject);

            scrollAction.Disable();
        }

        void OnScroll(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Performed)
            {
                float d = value.ReadValue<Vector2>().y;

                //Move the selection by the direction of input
                internalScroll = Mathf.Clamp(internalScroll + Mathf.Sign(d), 0, panel.stackCount);
                //Allow for un equipping items
                selection = Mathf.RoundToInt(internalScroll) - 1;
                SetTitleText();
            }
        }
        void SetTitleText()
        {
            if (selection != -1)
            {
                selectedTitle.text = InventoryController.singleton.db[panel[selection].name].displayName;
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
            scroll = Mathf.SmoothDamp(scroll, selection, ref scrollVel, 0.1f, 10, Time.unscaledDeltaTime);

            UpdateChildGroup();
        }
        void UpdateChildGroup()
        {
            childGroup.anchoredPosition = -Vector2.right * (scroll * (optionSize.x + optionSpacing) - childGroup.sizeDelta.x * 0.5f + optionSize.x * 1.5f + optionSpacing);
        }


    }
}