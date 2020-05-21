using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelect : MonoBehaviour
{
    public RectTransform[] options;
    public Vector2 selectedSize = new Vector2(60, 50);
    public Vector2 deselectedSize = new Vector2(30, 25);
    public string InstanceName;
    int selected;

    private void OnValidate()
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null)
            {
                options[i].sizeDelta = deselectedSize;
            }
        }
    }

    public static Dictionary<string, ItemSelect> instances = new Dictionary<string, ItemSelect>();

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        instances[InstanceName] = this;
    }

    public static void SetInstanceItem(string instanceName, int selected)
    {
        if (instances.ContainsKey(instanceName))
            instances[instanceName].SetItem(selected);
    }
    void SetSize(int index, Vector2 size)
    {
        if (options[index] != null)
            options[index].sizeDelta = size;
    }
    public void SetItem(int newSelect)
    {
        if (newSelect >= options.Length)
            return;

        SetSize(selected, deselectedSize);
        SetSize(newSelect, selectedSize);

        selected = newSelect;
    }
}
