using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class IndicatorUI : MonoBehaviour
{
    protected Transform target = null;
    protected Vector3 worldOffset;
    new protected Camera camera;

    public void Init(Transform target, Vector3 worldOffset = default)
    {
        Assert.IsNotNull(target, "No target");
        if (target == null) enabled = false;
        this.target = target;
        this.worldOffset = worldOffset;
    }
    protected virtual void Start()
    {
        camera = Camera.main;
        Assert.IsNotNull(camera, "No Camera");
        if (camera == null) enabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //position self on target
        Vector3 screenPos = camera.WorldToScreenPoint(target.position + worldOffset);
        //Sort by distance
        screenPos.z = Vector3.SqrMagnitude(target.position + worldOffset - camera.transform.position);

        transform.position = screenPos;
    }
}
