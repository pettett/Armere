using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public abstract class SettingsFormatBase : System.Attribute
{

}

public class SliderUI : SettingsFormatBase
{
    public float min;
    public float max;
    public float interval;

    public SliderUI(float min, float max, float interval = 0)
    {
        this.min = min;
        this.max = max;
        this.interval = interval;
    }
}
public class DropdownUI : SettingsFormatBase
{
}


public class SettingsUIController : MonoBehaviour
{
    public RectTransform listTransform;
    public AudioClip sliderClip;
    AudioSource source;
    public Slider sliderPrefab;
    public TMPro.TMP_Dropdown dropdownPrefab;

    private void Start()
    {
        source = GetComponent<AudioSource>();

        var properties = typeof(SettingsConfig).GetFields();

        for (int i = 0; i < properties.Length; i++)
        {
            foreach (var a in properties[i].GetCustomAttributes(false))
            {
                if (a.GetType() == typeof(SliderUI))
                {
                    var s = Instantiate(sliderPrefab, listTransform);
                    s.maxValue = (a as SliderUI).max;
                    s.minValue = (a as SliderUI).min;
                    s.value = (float)properties[i].GetValue(SettingsManager.settings);

                    s.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = properties[i].Name;
                    int index = i;
                    float interval = (a as SliderUI).interval;
                    if ((a as SliderUI).interval != 0)
                        s.onValueChanged.AddListener(
                            (float newValue) =>
                            {
                                s.value = Mathf.Round(newValue / interval) * interval; //round the new value into the interval
                                properties[index].SetValue(SettingsManager.settings, s.value); //update it in the settings
                                                                                               // source.PlayOneShot(sliderClip);
                            });
                    else
                        s.onValueChanged.AddListener((float newValue) => properties[index].SetValue(SettingsManager.settings, newValue));

                    s.onValueChanged.AddListener((float newValue) => OnChange());

                }
                else if (a.GetType() == typeof(DropdownUI))
                {
                    var s = Instantiate(dropdownPrefab, transform);
                    s.options = new List<TMPro.TMP_Dropdown.OptionData>();

                    foreach (string name in System.Enum.GetNames(properties[i].FieldType))
                    {
                        s.options.Add(new TMPro.TMP_Dropdown.OptionData(name));
                    }
                    s.value = (int)properties[i].GetValue(SettingsManager.settings);

                    int index = i;
                    s.onValueChanged.AddListener((int newValue) => properties[index].SetValue(SettingsManager.settings, newValue));
                    s.onValueChanged.AddListener((int newValue) => OnChange());
                }
            }
        }

    }


    public void OnChange()
    {
        SaveManager.SaveData(SettingsManager.settings,
                             SettingsManager.settingsFile);
    }

}
