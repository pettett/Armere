using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
public class BuyInventoryUIItem : MonoBehaviour
{
    public TextMeshProUGUI title;
    public CurrencyDisplay cost;
    public TextMeshProUGUI stock;
    public Image thumbnail;
    public Button selectButton;
    public BuyInventoryUI controller;
    public int index;
    public Color enoughMoneyColor = Color.black;
    public Color notEnoughMoneyColor = Color.red;
    public void UpdateCost(uint itemCost, uint playerCurrency)
    {
        bool enoughCurrency = playerCurrency >= itemCost;
        cost.SetCurrencyDisplay(
            itemCost,
             enoughCurrency ? enoughMoneyColor : notEnoughMoneyColor
            );

        selectButton.interactable = enoughCurrency;
    }
    public void OnSelect()
    {
        controller.ShowInfo(index);
    }
    public void OnPointerOver()
    {
        controller.ShowInfo(index);
    }
}
