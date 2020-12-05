using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    private void Start()
    {
        if (syncInventoryCurrency)
            InventoryController.singleton.currency.onPanelUpdated += OnCurrencyUpdated;
    }

    public void SetCurrencyDisplay(uint currency) => text.text = string.Format(format, currency);

    public void SetCurrencyDisplay(uint currency, Color color)
    {
        text.text = string.Format(format, currency);
        text.color = color;
    }
    void OnCurrencyUpdated(InventoryController.InventoryPanel panel)
    {
        //Show animation, update value
        //Wait a bit, hide again
        gameObject.SetActive(true);
        StartCoroutine(DisplayCurrencyAnimation());
    }
    IEnumerator DisplayCurrencyAnimation()
    {
        float time = 0;
        uint newCurrency = InventoryController.singleton.currency.ItemCount(0);
        while (time < currencyChangeTime)
        {
            time += Time.deltaTime;
            SetCurrencyDisplay((uint)Mathf.RoundToInt(Mathf.Lerp(currentCurrency, newCurrency, time / currencyChangeTime)));

            yield return new WaitForEndOfFrame();
        }
        currentCurrency = newCurrency;

        while (time < currencyChangeTime + updateTime)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
