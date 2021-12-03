using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
	public static LevelInfo currentLevelInfo;
	public GameObject playerPrefab;
	public GameObject player;
	public Transform playerTransform => player == null ? null : player.transform;


	public string currentRegionName;

	private void OnAfterGameLoaded()
	{
		Debug.Log("Spawning player", this);
		player = Instantiate(playerPrefab);
	}
	private void OnDestroy()
	{
		if (currentLevelInfo == this)
			currentLevelInfo = null;
	}

	private void Awake()
	{
		currentLevelInfo = this;
	}



}
