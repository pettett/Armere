using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassDeformCamera : MonoBehaviour
{
	public Transform followObject;
	// Start is called before the first frame update

	private void LateUpdate()
	{
		transform.position = followObject.position;
	}
}
