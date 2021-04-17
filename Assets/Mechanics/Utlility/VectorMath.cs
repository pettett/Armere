using UnityEngine;
public static class VectorMath
{

	public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		Vector3 AB = b - a;
		Vector3 AV = value - a;
		return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
	}

	public static float InverseLerp(Vector2 a, Vector2 b, Vector2 value)
	{
		Vector2 AB = b - a;
		Vector2 AV = value - a;
		return Vector2.Dot(AV, AB) / Vector2.Dot(AB, AB);
	}

	public static Vector3 ClosestPointOnPath(Vector3[] path, Vector3 point)
	{
		Vector3 closestPoint = default;
		float closestSqrDistance = Mathf.Infinity;
		for (int i = 0; i < path.Length - 1; i++)
		{
			Vector3 pos = Vector3.Lerp(path[i], path[i + 1], InverseLerp(path[i], path[i + 1], point));
			if ((pos - point).sqrMagnitude < closestSqrDistance)
			{
				closestSqrDistance = (pos - point).sqrMagnitude;
				closestPoint = pos;
			}
		}

		return closestPoint;
	}

	public static Vector2 ClosestPointOnPath(Vector2[] path, Vector2 point, bool loop)
	{
		Vector2 closestPoint = default;
		float closestSqrDistance = Mathf.Infinity;
		Vector2 pos;
		void TestPoint()
		{
			if ((pos - point).sqrMagnitude < closestSqrDistance)
			{
				closestSqrDistance = (pos - point).sqrMagnitude;
				closestPoint = pos;
			}
		}
		for (int i = 0; i < path.Length - 1; i++)
		{
			pos = Vector2.Lerp(path[i], path[i + 1], InverseLerp(path[i], path[i + 1], point));
			TestPoint();
		}
		if (loop)
		{
			//Also test between first and last
			pos = Vector2.Lerp(path[0], path[path.Length - 1], InverseLerp(path[0], path[path.Length - 1], point));
			TestPoint();
		}

		return closestPoint;
	}

}