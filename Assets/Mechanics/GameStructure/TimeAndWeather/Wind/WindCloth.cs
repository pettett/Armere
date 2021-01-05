using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindCloth : MonoBehaviour
{
    public Cloth cloth;

    public void FixedUpdate()
    {
        cloth.externalAcceleration = TimeDayController.singleton.Wind;
    }
}
