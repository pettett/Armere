using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FocusableIndicatorUI : IndicatorUI
{
    public Graphic focusStateGraphic;
    public Color focusedCol;
    public Color unFocusedCol;
    protected override void Start()
    {
        SetUnFocused();
        base.Start();
    }
    public void SetFocused()
    {
        focusStateGraphic.color = focusedCol;
    }
    public void SetUnFocused()
    {
        focusStateGraphic.color = unFocusedCol;
    }

}
