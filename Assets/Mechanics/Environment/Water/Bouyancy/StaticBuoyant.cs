using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBuoyant : MonoBehaviour
{
	public float maxRoll = 10;
	public float maxPitch = 10;
	public float timeScale = 0.01f;
	float yaw;
	private void Start()
	{
		yaw = transform.eulerAngles.y;
	}
	private void Update()
	{
		float roll = Mathf.PerlinNoise(Time.time * timeScale, 0) * 2 - 1;
		float pitch = Mathf.PerlinNoise(0, Time.time * timeScale) * 2 - 1;

		transform.eulerAngles = new Vector3(roll, yaw, pitch);

	}
}
