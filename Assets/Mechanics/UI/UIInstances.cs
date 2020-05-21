using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIInstances<T> : MonoBehaviour where T : MonoBehaviour
{

    public string instanceName = "health";

    public static Dictionary<string, T> instances = new Dictionary<string, T>();
    private void Awake()
    {
        instances[instanceName] = this as T;
    }


}