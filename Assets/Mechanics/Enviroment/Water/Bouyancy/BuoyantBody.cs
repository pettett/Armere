using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuoyantBody : MonoBehaviour, IWaterObject
{
    public float density = 700f;
    public float waterDrag = 0.9f;
    protected Rigidbody rb;
    protected WaterController volume;
    public void OnWaterEnter(WaterController waterController)
    {
        volume = waterController;
    }

    public void OnWaterExit(WaterController waterController)
    {
        volume = null;
    }
}
