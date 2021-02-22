using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
	public static LevelInfo currentLevelInfo;
	public string levelName;
	public GameObject player;
	public Transform playerTransform => player.transform;


	public string currentRegionName;

	private void Start()
	{
		LevelManager.singleton.currentLevel = levelName;
	}

	private void Awake()
	{
		currentLevelInfo = this;
	}



}
