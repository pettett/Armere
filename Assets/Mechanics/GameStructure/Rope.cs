using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
	public Transform start;
	public Transform end;

	public float c;
	public float radius;

	public float cosh(float x)
	{
		return (Mathf.Exp(x) + Mathf.Exp(-x)) / 2;
	}

	public float sinh(float x)
	{
		return (Mathf.Exp(x) - Mathf.Exp(-x)) / 2;
	}



	(float x, float y) SampleRope(float x)
	{
		//float dydx = sinh(x / c);

		//Vector2 gradientDirection = new Vector2(dydx, 1).normalized * radius ;

		return (x, c * cosh(x / c));
	}


	private void OnDrawGizmos()
	{
		//Gizmos.DrawLine(start.position, end.position);


		Vector2 startHorizontal = new Vector2(start.position.x, start.position.z);
		Vector2 endHorizontal = new Vector2(end.position.x, end.position.z);

		float a = Vector2.Distance(startHorizontal, endHorizontal);
		float b = Mathf.Abs(start.position.y - end.position.y);

		float A = Mathf.Exp(a / c);


		float root = Mathf.Sqrt((4 * b * b * Mathf.Exp(2 * a / c)) / (c * c) - 4 * A * (1 - A) * (A - 1));

		float startX = c * Mathf.Log((root * Mathf.Exp(-a / c) - (2 * b) / c) / (2 * (A - 1)));




		//float startX = Mathf.Log(((1 / A) * Mathf.Sqrt(A * A * b * b + A - 2 * A * A + A * A * A) - b) / (A - 1));
		float endX = startX + a;

		if (Mathf.Sign(start.position.y - end.position.y) < 0)
		{
			(startX, endX) = (endX, startX);
		}


		float offset = -cosh(startX / c) * c;

		Vector2 dir = startHorizontal - endHorizontal;
		float length = dir.magnitude;
		float yRot = Vector2.SignedAngle(dir, Vector2.left);

		Gizmos.matrix = Matrix4x4.TRS(start.position, Quaternion.Euler(0, yRot, 0), Vector3.one);

		void Draw(float angle)
		{
			for (int i = 0; i < 10; i++)
			{
				float t1 = i / 10f;
				float t2 = (i + 1) / 10f;



				(float x1, float y1) = SampleRope(Mathf.Lerp(startX, endX, t1));
				(float x2, float y2) = SampleRope(Mathf.Lerp(startX, endX, t2));
				y1 += offset;
				y2 += offset;

				//Debug.Log(y1);
				float h1 = length * Mathf.InverseLerp(startX, endX, x1);
				float h2 = length * Mathf.InverseLerp(startX, endX, x2);

				Gizmos.DrawLine(
					new Vector3(h1, y1, 0),
					new Vector3(h2, y2, 0));
			}
		}
		//Draw(1);
		Draw(0);
		//Draw(-1);
	}
}
