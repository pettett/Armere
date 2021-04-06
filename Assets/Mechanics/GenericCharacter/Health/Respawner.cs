using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Health))]
public class Respawner : MonoBehaviour
{
	public VoidEventChannelSO respawnChannel;

	Health health;

	public bool respawnInTime = false;
	public float respawnTime = 5f;


	private void Start()
	{
		health = GetComponent<Health>();
		if (respawnChannel != null)
		{
			respawnChannel.OnEventRaised += Respawn;
		}
		if (respawnInTime)
			health.onDeathEvent.AddListener(RespawnInTime);

	}
	void RespawnInTime()
	{
		Invoke("Respawn", respawnTime);
	}
	void Respawn()
	{
		gameObject.SetActive(true);
		health.Respawn();
	}
}
