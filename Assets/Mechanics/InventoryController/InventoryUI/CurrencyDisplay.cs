using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Armere.Inventory.UI
{
    public class CurrencyDisplay : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public string format = "{0}<size=150%><voffset=-0.105em>¤";
        public bool syncInventoryCurrency = true;
        public float updateTime = 1.5f;
        public float currencyChangeTime = 0.5f;
        public Color enoughMoneyColor = Color.white;
        public Color notEnoughMoneyColor = Color.red;
        uint currentCurrency = 0;
        public bool animateOnCurrencyChange = true;
        private void Start()
        {

            if (syncInventoryCurrency)
            {
                SetCurrencyDisplay(InventoryController.singleton.currency.currency);
                InventoryController.singleton.currency.onPanelUpdated += OnCurrencyUpdated;
            }
        }

        public void SetCurrencyDisplay(uint currency) => text.SetText(string.Format(format, currency));

        public void SetCurrencyDisplay(uint currency, Color color)
        {
            SetCurrencyDisplay(currency);
            text.color = color;
        }
        void OnCurrencyUpdated(InventoryPanel panel)
        {
            //Show animation, update value
            //Wait a bit, hide again
            if (animateOnCurrencyChange)
            {
                gameObject.SetActive(true);
                StartCoroutine(DisplayCurrencyAnimation());
            }
            else
            {
                SetCurrencyDisplay(InventoryController.singleton.currency.currency);
            }
        }
        IEnumerator DisplayCurrencyAnimation()
        {
            float time = 0;
            uint newCurrency = InventoryController.singleton.currency.currency;
            while (time < currencyChangeTime)
            {
                time += Time.deltaTime;
                SetCurrencyDisplay((uint)Mathf.RoundToInt(Mathf.Lerp(currentCurrency, newCurrency, time / currencyChangeTime)));

                yield return null;
            }
            currentCurrency = newCurrency;

            while (time < currencyChangeTime + updateTime)
            {
                time += Time.deltaTime;
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}