using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorsUIController : MonoBehaviour
{
    public static IndicatorsUIController singleton;

    public GameObject alertIndicatorPrefab;

    //Used for camera locking in the future maybe
    public GameObject focusableIndicatorPrefab;

    private void Awake()
    {
        singleton = this;
    }

    public AlertIndicatorUI CreateAlertIndicator(Transform target, Vector3 worldOffset = default)
    {
        return (AlertIndicatorUI)CreateIndicator(target, alertIndicatorPrefab, worldOffset);
    }
    public FocusableIndicatorUI CreateFocusableIndicator(Transform target, Vector3 worldOffset = default)
    {
        return (FocusableIndicatorUI)CreateIndicator(target, focusableIndicatorPrefab, worldOffset);
    }
    public IndicatorUI CreateIndicator(Transform target, GameObject prefab, Vector3 worldOffset = default)
    {
        GameObject g = Instantiate(prefab, transform);
        var x = g.GetComponent<IndicatorUI>();
        x.Init(target, worldOffset);
        return x;
    }
}
