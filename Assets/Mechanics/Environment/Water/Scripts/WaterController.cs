
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.AddressableAssets;

public class WaterController : MonoBehaviour
{
	[System.Serializable]
	public struct WaterPathNode
	{
		public Transform transform;
		public float waterWidth;

		public float tangentStrength;
		[Range(0, 1)]
		public float edgeFlowStrength;
	}

	[System.Serializable]
	public struct WaterFlowSource
	{
		public Transform transform;
		public float strength;
		public float radius;
	}

	public WaterPathNode[] path = new WaterPathNode[0];
	public WaterFlowSource[] sources = new WaterFlowSource[0];
	public FluidTemplate template;
	public Collider waterVolume;

	public float flowRate = 1f;

	public Bounds bounds => waterVolume.bounds;
	public float density => template.density;





	static void DrawCross(Vector3 center, float size)
	{
		const float time = 2f;
		Debug.DrawLine(center + Vector3.up * size, center - Vector3.up * size, Color.green, time);
		Debug.DrawLine(center + Vector3.right * size, center - Vector3.right * size, Color.green, time);
		Debug.DrawLine(center + Vector3.forward * size, center - Vector3.forward * size, Color.green, time);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			other.GetComponent<IWaterObject>()?.OnWaterEnter(this);
			CreateSplash(other);
		}
	}


	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger)
		{
			other.GetComponent<IWaterObject>()?.OnWaterExit(this);
			CreateSplash(other);
		}
	}


	public Vector3 GetSurfacePosition(Vector3 inWaterPosition)
	{
		return waterVolume.ClosestPoint(inWaterPosition + Vector3.up * 10);
	}
	public Vector3 GetCollisionPosition(Vector3 otherCenter)
	{
		Vector3 collisionEstimate = waterVolume.ClosestPoint(otherCenter + Vector3.up);
		// collisionEstimate = other.ClosestPoint(collisionEstimate);
		//  collisionEstimate = waterVolume.ClosestPoint(collisionEstimate);
		DrawCross(collisionEstimate, 0.1f);
		return collisionEstimate;
	}

	void CreateSplash(Collider other) => CreateSplash(GetCollisionPosition(other.transform.position), Mathf.Clamp01(Mathf.Abs((other.attachedRigidbody?.velocity.y ?? 1) * 2)));

	public void CreateSplash(Vector3 position, float size = 1)
	{
		position.y += 0.03f;
		if (template.splashEffect != null)
		{
			template.splashEffect.transform.position = position;

			template.splashEffect.SetFloat("Splash Size", size);
			template.splashEffect.SendEvent("Splash");
		}
	}



	static Vector2 Flatten(Vector3 p)
	{
		return new Vector2(p.x, p.z);
	}
	static Vector2 Normal(Vector2 p1, Vector2 p2)
	{
		return Vector2.Perpendicular(p1 - p2).normalized;
	}
	static Vector2 PointNormal(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (Normal(p1, p2) + Normal(p2, p3)).normalized;
	}
	static Vector2 Tangent(Vector2 p1, Vector2 p2)
	{
		return (p1 - p2).normalized;
	}
	static Vector2 PointTangent(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (Tangent(p1, p2) + Tangent(p2, p3)).normalized;
	}
	public Vector3 GetTangent(int i)
	{
		Vector2 t;
		if (i == 0)
		{
			t = -Tangent(
				Flatten(path[i].transform.position),
				Flatten(path[i + 1].transform.position));
		}
		else if (i == path.Length - 1)
		{
			t = -Tangent(
				Flatten(path[i - 1].transform.position),
				Flatten(path[i].transform.position));
		}
		else
		{
			t = -PointTangent(
				Flatten(path[i - 1].transform.position),
				Flatten(path[i].transform.position),
				Flatten(path[i + 1].transform.position));
		}
		return new Vector3(t.x, 0, t.y);
	}
	public Vector3 Extrude(int i, float e)
	{
		Vector3 start = path[i].transform.position;
		return start + Vector3.Cross(GetTangent(i), Vector3.up) * e;
	}

	public Vector3[] GetExtrudedPath(float multiplier)
	{
		Vector3[] p = new Vector3[path.Length];
		for (int i = 0; i < path.Length; i++)
			p[i] = Extrude(i, path[i].waterWidth * multiplier);
		return p;
	}
	public Vector3 SampleBezier(Vector3 start, Vector3 end, Vector3 startHint, Vector3 endHint, float t)
	{
		float j = 1 - t;
		return j * j * j * start + 3 * j * j * t * startHint + 3 * j * t * t * endHint + t * t * t * end;
	}

	public Vector3 SampleBezierNormal(Vector3 start, Vector3 end, Vector3 startNormal, Vector3 endNormal, float t)
	{
		return SampleBezier(start, end, start + startNormal, end + endNormal, t);
	}



	public Vector3[] GetPath(int subDivisions)
	{
		Vector3[] p = new Vector3[path.Length + path.Length * subDivisions - subDivisions];
		int width = (subDivisions + 1);

		p[0] = path[0].transform.position;
		for (int i = 0; i < path.Length - 1; i++)
		{
			//p[i * width] = path[i].transform.position;

			//print(p[i]);


			Vector3 ti = GetTangent(i);
			Vector3 ti1 = GetTangent(i + 1);


			float repr = 1f / width;
			for (int j = 1; j <= width; j++)
			{
				//print(repr * j);
				p[i * width + j] = SampleBezierNormal(path[i].transform.position, path[i + 1].transform.position, ti * 5, ti1 * 5, repr * j) + Vector3.up * j;

				//print(p[i + j]);
			}


		}
		return p;
	}

	public int subDivisions = 3;
	private void OnDrawGizmos()
	{
		if (path.Length <= 1)
		{
			return;
		}

		var l = GetExtrudedPath(1);
		var r = GetExtrudedPath(-1);

		Gizmos.color = Color.cyan;


		Vector3[] p = new Vector3[path.Length + path.Length * subDivisions - subDivisions];
		int width = (subDivisions + 1);

		//print(p.Length);
		p[0] = path[0].transform.position;
		for (int i = 0; i < path.Length - 1; i++)
		{
			//p[i * width] = path[i].transform.position;

			//print(p[i]);


			Vector3 ti = GetTangent(i);
			Vector3 ti1 = GetTangent(i + 1);


			float repr = 1f / width;
			for (int j = 1; j <= width; j++)
			{
				float t = repr * j;

				//print(t);
				int index = i * width + j;

				p[index] = SampleBezierNormal(path[i].transform.position, path[i + 1].transform.position, ti * path[i].tangentStrength, -ti1 * path[i + 1].tangentStrength, t);

				Gizmos.DrawLine(path[i].transform.position, path[i].transform.position + ti * path[i].tangentStrength);
				//print(p[i + j]);
			}
		}

		Gizmos.color = Color.green;
		for (int i = 0; i < p.Length - 1; i++)
		{
			Gizmos.DrawLine(p[i], p[i + 1]);
		}

		Gizmos.color = Color.blue;
		for (int i = 0; i < path.Length - 1; i++)
		{
			Gizmos.DrawLine(l[i], l[i + 1]);
			Gizmos.DrawLine(r[i], r[i + 1]);
		}


		Gizmos.color = Color.red;

		for (int i = 0; i < sources.Length; i++)
		{
			Gizmos.DrawWireSphere(sources[i].transform.position, sources[i].radius);
		}
	}



}