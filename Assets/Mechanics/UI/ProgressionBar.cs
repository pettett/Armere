using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ProgressionBar : MonoBehaviour
{
    public TextMeshProUGUI display;
    public string displayFormat = "n1";
    public Image displayImage;
    public string instanceName = "health";

    public static Dictionary<string, ProgressionBar> instances = new Dictionary<string, ProgressionBar>();
    private void Awake()
    {
        instances[instanceName] = this;
    }

    public static void SetInstanceProgress(string instanceName, float progress, float max)
    {
        if (instances.ContainsKey(instanceName))
            instances[instanceName].SetProgress(progress, max);
    }
    public void SetProgress(float progress, float max)
    {
        display.text = progress.ToString(displayFormat);
        displayImage.fillAmount = progress / max;
    }
}
