using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldIndicator : IndicatorUI
{

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    public StringEvent onIndicate;
    public UnityEvent onEndIndicate;
    public void StartIndication(Transform target, string title, Vector3 worldOffset = default)
    {
        Init(target, worldOffset);

        onIndicate?.Invoke(title);
        gameObject.SetActive(true);
    }
    public void EndIndication()
    {
        target = null;
        onEndIndicate?.Invoke();
        gameObject.SetActive(false);
    }




}
