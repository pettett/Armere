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
public class PlayerVision : Vision
{
	new Camera camera;

	public override float FOV => camera.fieldOfView;

	public override float Aspect => camera.aspect;

	public override Vector3 ViewPoint => camera.transform.position;

	public List<IVisable> visionGroup = new List<IVisable>();
	private void Start()
	{
		camera = GetComponent<Camera>();
	}
	public virtual void EnterVision(IVisable visable)
	{
		visable.OnEnterVision();
	}
	public virtual void StayVision(IVisable visable, float center, float distance)
	{
		visable.OnStayVision(center, distance);
	}
	public virtual void ExitVision(IVisable visable)
	{
		visable.OnExitVision();
	}


	private void Update()
	{

		Matrix4x4 viewMatrix = Matrix4x4.Perspective(FOV, Aspect, clippingPlanes.Min, clippingPlanes.Max) * Matrix4x4.Scale(new Vector3(1, 1, -1)) * transform.worldToLocalMatrix;
		//GeometryUtility.CalculateFrustumPlanes(viewMatrix, viewPlanes);

		for (int i = 0; i < visionGroup.Count; i++)
		{
			Vector3 point = visionGroup[i].VisionPoint;
			point = viewMatrix.MultiplyPoint(point);

			if (point.z < 1 && point.z > 0 && Mathf.Abs(point.x) < 1 && Mathf.Abs(point.y) < 1)
			{
				//Inside vision square
				if (!visionGroup[i].inVision)
					EnterVision(visionGroup[i]);//.OnEnterVision();

				StayVision(visionGroup[i], 1 - Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y)), point.z);
			}
			else if (visionGroup[i].inVision)
			{
				ExitVision(visionGroup[i]);//.OnExitVision();
			}
		}

	}

	public override float ProportionBoundsVisible(Bounds b)
	{
		return 1;
	}
}