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
        GameObject g = Instantiate(alertIndicatorPrefab, transform);
        var x = g.GetComponent<AlertIndicatorUI>();
        x.Init(target, worldOffset);
        return x;
    }
}
