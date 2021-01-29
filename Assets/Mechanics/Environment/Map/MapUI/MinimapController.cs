using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MinimapController : MapUI
{

	protected override void Update()
	{
		//Move the map object to focus on player's position
		Vector3 trackPos = LevelInfo.currentLevelInfo.playerTransform.position;
		map.anchoredPosition = new Vector2(trackPos.x, trackPos.z) * mapScale;


		float playerLookDir = 180 - LevelInfo.currentLevelInfo.playerTransform.eulerAngles.y;
		float cameraLookDir = 180 - Camera.main.transform.eulerAngles.y;

		playerPositionIndicator.eulerAngles = Vector3.forward * playerLookDir;
		playerViewIndicator.eulerAngles = Vector3.forward * cameraLookDir;


		base.Update();
	}
}
