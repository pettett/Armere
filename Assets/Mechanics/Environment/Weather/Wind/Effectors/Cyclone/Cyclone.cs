using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CapsuleCollider))]
public class Cyclone : MonoBehaviour
{

	public float height = 10f;

	public int layers = 10;

	public float rotationSpeed = 1f;
	public float spinSpeed = 20f;
	public float phaseDifference = 0.2f;
	public float rotationScale = 0.2f;

	public AnimationCurve sizeOverHeight = AnimationCurve.EaseInOut(0, 0, 1, 1);
	float maxRadius = -1;
	new CapsuleCollider collider;

	public Vector3 forces = new Vector3(15, 30, 20);

	List<Rigidbody> contained = new();

	public Transform rootBone;

	// Start is called before the first frame update
	void Start()
	{
		maxRadius = sizeOverHeight.keys.Max(k => k.value) * (1 + rotationScale);
	}

	private void OnValidate()
	{
		collider = GetComponent<CapsuleCollider>();

		collider.center = Vector3.up * height * 0.5f;

		maxRadius = sizeOverHeight.keys.Max(k => k.value) * (1 + rotationScale);

		//Radius of collider is greatest radius
		collider.radius = maxRadius;

		collider.height = height + 2 * maxRadius;
	}

	// Update is called once per frame
	void Update()
	{
		rootBone.rotation = Quaternion.AngleAxis(Time.time * spinSpeed, Vector3.down * Mathf.Sign(rotationSpeed));

		Transform currentBone = rootBone;

		float runningScale = 1;

		for (int i = 0; i < layers; i++)
		{
			currentBone.position = GetCenter(i, out float radius);

			//Scale is inherited from bone before, so calculate the relitive change in scale
			float scaleChange = radius / runningScale;

			currentBone.localScale = new Vector3(scaleChange, 1, scaleChange);

			runningScale *= scaleChange;

			currentBone = currentBone.GetChild(0);
		}

	}

	private void OnGUI()
	{
		if (GUILayout.Button("Spawn cube"))
		{
			var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
			c.transform.position = transform.position + new Vector3(5, 2, 2);
			c.AddComponent<Rigidbody>();
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		contained.Add(other.attachedRigidbody);
	}
	private void OnTriggerExit(Collider other)
	{
		contained.Remove(other.attachedRigidbody);
	}

	void CalcForce(Rigidbody obj)
	{
		Transform t = obj.transform;

		Vector3 comparison = Vector3.down * Mathf.Sign(rotationSpeed);

		Vector3 direction = (transform.position - t.position);
		direction.y = 0;

		float r = direction.magnitude;

		direction /= r;

		float centred = 1 - r / maxRadius;

		Vector3 tangent = Vector3.Cross(direction, comparison);

		//We want a consant speed to orbit 
		float tangentComponent = Vector3.Dot(tangent, obj.velocity);


		obj.AddForce(tangent * (forces.x - tangentComponent));
		obj.AddForce(Vector3.up * forces.y);
		obj.AddForce(direction * forces.z);

		obj.AddTorque(comparison * centred);
	}

	void DrawGizmoFor(Rigidbody obj)
	{
		Transform t = obj.transform;

		Vector3 direction = (transform.position - t.position);
		direction.y = 0;
		direction.Normalize();

		Vector3 tangent = Vector3.Cross(direction, Vector3.up);



		Gizmos.DrawLine(t.position, t.position + tangent * forces.x);
		Gizmos.DrawLine(t.position, t.position + Vector3.up * forces.y);
		Gizmos.DrawLine(t.position, t.position + direction * forces.z);

	}

	private void FixedUpdate()
	{
		for (int i = 0; i < contained.Count; i++)
		{
			CalcForce(contained[i]);
		}
	}



	private void DrawCircle(Vector3 center, float radius)
	{

		for (int i = 0; i < 12; i++)
		{
			float p1 = Mathf.PI * 2f * i / 12f;
			float p2 = Mathf.PI * 2f * (i + 1) / 12f;
			Gizmos.DrawLine(
				center + new Vector3(Mathf.Sin(p1), 0, Mathf.Cos(p1)) * radius,
			 	center + new Vector3(Mathf.Sin(p2), 0, Mathf.Cos(p2)) * radius
			 );

		}
	}

	public Vector3 GetCenter(int layer, out float radius)
	{
		return transform.TransformPoint(GetLocalCenter(layer, out radius));
	}

	public Vector3 GetLocalCenter(int layer, out float radius)
	{
		float gap = height / layers;
		float inteval = 1 / (float)(layers - 1);

		radius = sizeOverHeight.Evaluate(inteval * layer);

		float angle = phaseDifference * layer + Time.time * rotationSpeed;

		Vector3 center = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) / radius * rotationScale;
		center.y = gap * layer;

		return center;
	}

	private void OnDrawGizmos()
	{
		//Draw forces of contained objects
		Gizmos.color = Color.red;
		for (int i = 0; i < contained.Count; i++)
		{
			DrawGizmoFor(contained[i]);
		}


		float gap = height / layers;

		Gizmos.matrix = transform.localToWorldMatrix;

		float inteval = 1 / (float)(layers - 1);

		float lastRad = default;
		Vector3 lastCentre = default;

		//Draw Lines outlining
		for (int i = 0; i < layers; i++)
		{

			Vector3 center = GetLocalCenter(i, out float radius);

			if (i > 0)
			{
				//Draw lines connecting layers 
				Gizmos.color = Color.blue;


				Gizmos.DrawLine(lastCentre, center);
				Gizmos.DrawLine(lastCentre + Vector3.left * lastRad, center + Vector3.left * radius);
				Gizmos.DrawLine(lastCentre - Vector3.left * lastRad, center - Vector3.left * radius);

				Gizmos.DrawLine(lastCentre + Vector3.forward * lastRad, center + Vector3.forward * radius);
				Gizmos.DrawLine(lastCentre - Vector3.forward * lastRad, center - Vector3.forward * radius);
			}

			//Draw lines on layer
			Gizmos.color = Color.green;

			DrawCircle(center, radius);

			lastRad = radius;
			lastCentre = center;
		}


		//Draw circles of body
		for (int i = 0; i < layers; i++)
		{
		}
	}
}
