using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsConfig
{
    [SliderUI(50, 120, 1)]
    public float fov = 80;
    [SliderUI(0.0001f, 100, 0.5f)]
    public float sensitivity = 10;

}

public static class SettingsManager
{
    public const string settingsFile = "settings";
    static SettingsConfig loadedSettings = null;
    public static SettingsConfig settings
    {
        get
        {
            if (loadedSettings == null)
                loadedSettings = SaveManager.LoadData<SettingsConfig>(settingsFile);
            return loadedSettings;
        }
    }

}
