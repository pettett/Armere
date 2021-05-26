using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flyer : MonoBehaviour
{
	struct Panic
	{
		public Vector3 pos;
		public float startTime;
	}
	FlyerTemplate template;
	Transform centerPos;
	float seed;
	public VirtualAudioListener audioListener;

	Panic? panic;

	// Start is called before the first frame update
	private void Start()
	{
		if (audioListener != null)
			audioListener.onHearNoise += OnHearNoise;
	}
	public void OnHearNoise(Vector3 pos)
	{
		panic = new Panic()
		{
			pos = pos,
			startTime = Time.time,
		};
	}

	public void Init(FlyerTemplate template, Transform spawn)
	{
		this.template = template;
		this.centerPos = spawn;
		this.seed = Random.value * 10000;
	}
	void Disappear()
	{
		LeanTween.scale(gameObject, Vector3.zero, 0.2f).setOnComplete(
			() =>
			{
				Destroy(gameObject);
			}
		);
	}
	Vector3 accel;
	Vector3 lastVel;
	// Update is called once per frame
	void Update()
	{
		Vector3 vel;

		if (panic.HasValue)
		{
			Vector3 dir = (transform.position - panic.Value.pos).normalized;

			vel = dir * template.panicSpeed;



			if (Time.time - panic.Value.startTime > template.panicTime)
			{
				if (template.dissapearAfterPanic)
				{
					Disappear();
				}
				else
				{
					panic = null;
				}
			}
		}
		else
		{
			Vector3 p = transform.position;
			p += (Time.time + seed) * Vector3.one;
			p *= template.noiseScale;

			float y = Mathf.PerlinNoise(p.x, p.z) * 360;
			float x = Mathf.PerlinNoise(p.z, p.y) * 360;

			Vector3 noiseForce = Quaternion.Euler(x, y, 0) * Vector3.forward * template.noiseForce;


			Vector3 centerForce = (centerPos.position - transform.position) * template.centerForce;

			Vector3 accel = noiseForce + centerForce;

			vel = accel * template.speed;
		}

		lastVel = Vector3.SmoothDamp(lastVel, vel, ref accel, template.velocitySmoothTime);
		transform.position += lastVel * Time.deltaTime;
		transform.forward = lastVel;
	}
}
