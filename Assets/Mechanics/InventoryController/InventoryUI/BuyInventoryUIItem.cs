using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
public class BuyInventoryUIItem : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI stock;
    public Image thumbnail;
    public Button selectButton;
    public BuyInventoryUI controller;
    public int index;
    public void OnSelect()
    {
        controller.ShowInfo(index);
    }
    public void OnPointerOver()
    {
        controller.ShowInfo(index);
    }
}
