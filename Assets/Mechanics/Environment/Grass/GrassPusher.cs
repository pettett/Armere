using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassPusher : MonoBehaviour
{
    public static GrassPusher mainPusher;
    public float radius;
    public Vector3 offset;
    public Vector4 Data => new Vector4(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z, radius);

    public bool setMainPusher = false;
    private void OnEnable()
    {
        GrassController.pushers.Add(this);
        if (setMainPusher)
        {
            mainPusher = this;
        }
    }
    private void OnDisable()
    {
        GrassController.pushers.Remove(this);
        if (setMainPusher && mainPusher == this)
        {
            mainPusher = null;
        }
    }
}
