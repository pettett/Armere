using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBody : MonoBehaviour
{
	public GlobalVector3SO windDirectionGlobal;

	public float multiplier = 1f;

	Rigidbody rigidbody;
	private void Start()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		rigidbody.AddForce(windDirectionGlobal.value * multiplier);
	}
}
