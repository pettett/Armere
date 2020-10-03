using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorUI : MonoBehaviour
{
    protected Transform target = null;
    protected Vector3 worldOffset;
    new protected Camera camera;

    public void Init(Transform target, Vector3 worldOffset = default)
    {
        this.target = target;
        this.worldOffset = worldOffset;
    }
    private void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            //position self on target
            Vector3 screenPos = camera.WorldToScreenPoint(target.position + worldOffset);
            transform.position = screenPos;
        }
    }
}
