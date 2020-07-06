using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogController : MonoBehaviour
{
    public float fog;
    private void Update()
    {
        RenderSettings.fogDensity = fog;
    }
}
