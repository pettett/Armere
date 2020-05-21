using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStyle : MonoBehaviour
{

    [ColorUsageAttribute(false, false)] public Color numberColor = Color.red;
    public string numberColorHex => ColorUtility.ToHtmlStringRGB(numberColor);
    [ColorUsageAttribute(false, false)] public Color itemNameColor = Color.green;
    public string itemNameColorHex => ColorUtility.ToHtmlStringRGB(itemNameColor);
    [ColorUsageAttribute(false, false)] public Color NPCNameColor = Color.blue;
    public string NPCNameColorHex => ColorUtility.ToHtmlStringRGB(NPCNameColor);
}
