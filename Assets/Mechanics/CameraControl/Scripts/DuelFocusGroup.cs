using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class DuelFocusGroup : MonoBehaviour, ICinemachineTargetGroup
{
	public Transform mainTarget;
	public Transform focusTarget;
	public float weight;

	public float mainRadius = 1;
	public float focusRadius = 1;


	public Transform Transform => transform;

	public Bounds BoundingBox { get; private set; }

	public BoundingSphere Sphere
	{
		get
		{
			Bounds b = BoundingBox;
			return new BoundingSphere(b.center, ((b.max - b.min) / 2).magnitude);
		}
	}


	public bool IsEmpty => mainTarget == null || focusTarget == null;


	BoundingSphere MainBounds => new BoundingSphere(mainTarget.position, mainRadius);
	BoundingSphere FocusBounds => new BoundingSphere(focusTarget.position, focusRadius * weight);


	public void GetViewSpaceAngularBounds(
		Matrix4x4 observer, out Vector2 minAngles, out Vector2 maxAngles, out Vector2 zRange)
	{
		zRange = Vector2.zero;

		Matrix4x4 inverseView = observer.inverse;
		Bounds b = new Bounds();
		bool haveOne = false;

		void AddBoundingSphere(BoundingSphere s, ref Vector2 zRanges)
		{
			Vector3 p = inverseView.MultiplyPoint3x4(s.position);

			var r = s.radius / p.z;
			var r2 = new Vector3(r, r, 0);
			var p2 = p / p.z;
			if (!haveOne)
			{
				b.center = p2;
				b.extents = r2;
				zRanges = new Vector2(p.z - s.radius, p.z + s.radius);
				haveOne = true;
			}
			else
			{
				b.Encapsulate(p2 + r2);
				b.Encapsulate(p2 - r2);
				zRanges.x = Mathf.Min(zRanges.x, p.z - s.radius);
				zRanges.y = Mathf.Max(zRanges.y, p.z + s.radius);
			}
		}

		AddBoundingSphere(MainBounds, ref zRange);
		AddBoundingSphere(FocusBounds, ref zRange);

		// Don't need the high-precision versions of SignedAngle
		var pMin = b.min;
		var pMax = b.max;
		minAngles = new Vector2(
			Vector3.SignedAngle(Vector3.forward, new Vector3(0, pMin.y, 1), Vector3.left),
			Vector3.SignedAngle(Vector3.forward, new Vector3(pMin.x, 0, 1), Vector3.up));
		maxAngles = new Vector2(
			Vector3.SignedAngle(Vector3.forward, new Vector3(0, pMax.y, 1), Vector3.left),
			Vector3.SignedAngle(Vector3.forward, new Vector3(pMax.x, 0, 1), Vector3.up));
	}

	Bounds CalculateBoundingBox(Vector3 avgPos)
	{
		Bounds b = new Bounds(avgPos, Vector3.zero);

		b.Encapsulate(new Bounds(MainBounds.position, MainBounds.radius * 2 * Vector3.one));
		var f = FocusBounds;
		b.Encapsulate(new Bounds(f.position, f.radius * 2 * Vector3.one));

		return b;
	}

	public Bounds GetViewSpaceBoundingBox(Matrix4x4 observer)
	{
		Matrix4x4 inverseView = observer.inverse;
		Bounds b = new Bounds(inverseView.MultiplyPoint3x4(transform.position), Vector3.zero);


		Vector3 position = inverseView.MultiplyPoint3x4(mainTarget.transform.position);
		b.Encapsulate(new Bounds(position, mainRadius * 2 * Vector3.one));

		var f = FocusBounds;
		position = inverseView.MultiplyPoint3x4(f.position);
		b.Encapsulate(new Bounds(position, f.radius * 2 * Vector3.one));

		return b;
	}


	private void LateUpdate()
	{
		if (!IsEmpty)
		{
			transform.position = Vector3.Lerp(mainTarget.position, focusTarget.position, weight);
			BoundingBox = CalculateBoundingBox(transform.position);
		}

	}
	private void OnDrawGizmos()
	{
		if (!IsEmpty)
		{
			Gizmos.DrawLine(mainTarget.position, focusTarget.position);
			Gizmos.DrawWireSphere(MainBounds.position, MainBounds.radius);
			Gizmos.DrawWireSphere(FocusBounds.position, FocusBounds.radius);
		}
	}
}
