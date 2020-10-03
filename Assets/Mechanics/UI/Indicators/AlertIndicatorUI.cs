using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AlertIndicatorUI : IndicatorUI
{
    public Image alert;
    public Image investigate;

    public void SetInvestigation(float amount)
    {
        investigate.fillAmount = amount;
    }
    public void EnableAlert(bool enabled)
    {
        alert.gameObject.SetActive(enabled);
    }
    public void EnableInvestigate(bool enabled)
    {
        investigate.transform.parent.gameObject.SetActive(enabled);
    }
}
