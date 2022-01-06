using System.Collections.Generic;
using UnityEngine;

public class VirtualVision : Vision
{
	public Transform eye;
	public float fov = 30;
	public float aspect = 16f / 10f;

	public override float FOV => fov;

	public override float Aspect => aspect;

	public override Vector3 ViewPoint => eye.position;

	readonly Plane[] planes = new Plane[6];
	public LayerMask visionBlockingMask = -1;

	public void UpdatePlanes()
	{
		var viewMatrix = Matrix4x4.Perspective(FOV, 1, clippingPlanes.Min, clippingPlanes.Max) * Matrix4x4.Scale(new Vector3(1, 1, -1));
		GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, planes);
	}

	public bool CanSeeBounds(Bounds b)
	{
		return GeometryUtility.TestPlanesAABB(planes, b);
	}


	public bool TryFindTarget(Character character, out Character target)
	{
		UpdatePlanes();

		Team[] enemies = character.Enemies();


		for (int i = 0; i < enemies.Length; i++)
		{
			if (Character.teams.TryGetValue(enemies[i], out var targets))
			{

				for (int j = 0; j < targets.Count; j++)
				{
					var b = targets[j].bounds;
					if (CanSeeBounds(b))
					{
						// if (ProportionBoundsVisible(b) != 0)
						// {
						//can see the player, interrupt current routine
						target = targets[j];
						return true;
						// }
					}
				}
			}
		}
		target = null;
		return false;

	}


	public override float ProportionBoundsVisible(Bounds b)
	{

		float visibility = 0;
		int samples = 2;

		for (int i = 0; i < samples; i++)
		{
			Vector3 testPoint = b.center;
			testPoint.y += b.size.y * (i / (samples - 1f)) - b.extents.y;

			foreach (var plane in planes)
			{
				if (!plane.GetSide(testPoint))
				{
					//This point is not inside frustum, ignore it
					goto SkipPoint;
				}
			}

			//Line cast to point
			if (!Physics.Linecast(eye.position, testPoint, out RaycastHit hit, visionBlockingMask, QueryTriggerInteraction.Ignore))
			{
				//Add to visibility
				visibility += 1f / samples;

			}

		SkipPoint:
			continue;

		}

		return visibility;


	}
	private void OnDrawGizmos()
	{
		UpdatePlanes();

		foreach (var targets in Character.teams.Values)
		{
			for (int j = 0; j < targets.Count; j++)
			{
				var b = targets[j].bounds;

				float visibility = ProportionBoundsVisible(b);
				if (CanSeeBounds(b))
				{
					Gizmos.color = new Color(visibility, 0, 0);
					Gizmos.DrawWireCube(b.center, b.size);
				}

			}
		}



		Gizmos.color = Color.white;
		Gizmos.matrix = eye.localToWorldMatrix;
		Gizmos.DrawFrustum(Vector3.zero, fov, clippingPlanes.Max, clippingPlanes.Min, Aspect);

	}


}