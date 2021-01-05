using System.Collections.Generic;
using UnityEngine;

public interface IVisable
{
    //Should return 
    Vector3 VisionPoint { get; }
    void OnEnterVision();
    void OnStayVision(float center, float distance);
    void OnExitVision();
    bool inVision { get; }
}
public class VirtualVision : MonoBehaviour
{
    public Vector2 clippingPlanes = new Vector2(0.1f, 10f);
    public float fov = 30;
    public float aspect = 16f / 10f;
    public bool syncWithCamera;
    new Camera camera;
    public List<IVisable> visionGroup = new List<IVisable>();

    public float FOV
    {
        get
        {
            if (syncWithCamera) return camera.fieldOfView;
            else return fov;
        }
    }
    public float Aspect
    {
        get
        {
            if (syncWithCamera) return camera.aspect;
            else return aspect;
        }
    }
    //Plane[] viewPlanes = new Plane[6];

    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {

        Matrix4x4 viewMatrix = Matrix4x4.Perspective(FOV, Aspect, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1)) * transform.worldToLocalMatrix;
        //GeometryUtility.CalculateFrustumPlanes(viewMatrix, viewPlanes);

        for (int i = 0; i < visionGroup.Count; i++)
        {
            Vector3 point = visionGroup[i].VisionPoint;
            point = viewMatrix.MultiplyPoint(point);

            if (point.z < 1 && point.z > 0 && Mathf.Abs(point.x) < 1 && Mathf.Abs(point.y) < 1)
            {
                //Inside vision square
                if (!visionGroup[i].inVision)
                    visionGroup[i].OnEnterVision();

                visionGroup[i].OnStayVision(1 - Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y)), point.z);
            }
            else if (visionGroup[i].inVision)
            {
                visionGroup[i].OnExitVision();
            }
        }

    }
}