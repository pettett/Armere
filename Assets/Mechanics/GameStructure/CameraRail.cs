using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRail : MonoBehaviour
{
    //Make sure the camera is not clipping a block
    RaycastHit[] raycastHits = new RaycastHit[1];
    new Camera camera;
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                camera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView);
            halfExtends.x = halfExtends.y * camera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    private void Start()
    {
        camera = GetComponent<Camera>();
    }
    private void Update()
    {
        //boxcast from center to desired position

    }
}
