using UnityEngine;
using Cinemachine;
using System.Collections;
using Armere.PlayerController;

using Armere.UI;
public class SceneConnector : PlayerTrigger
{
	public string connectorIdentifier = "New Connector";
	public string exitConnection = "New Connector";
	public CinemachineVirtualCamera transitionToCamera;
	public float dollyTime = 2f;
	AutoWalking walker;
	public Transform endTransform;
	public Transform exitTransform;
	public string changeToLevel;
	public override void OnPlayerTrigger(PlayerController player)
	{
		StartSceneChange(player);
	}

	public void StartSceneChange(PlayerController player)
	{
		if (transitionToCamera != null)
		{
			transitionToCamera.Priority = 20;
			StartCoroutine(Dolly(player));
		}
		else
		{
			StartCoroutine(ChangeScene(player));
		}
		walker = (AutoWalking)player.machine.ChangeToState(player.autoWalk);
		walker.WalkTo(endTransform.position);
	}
	IEnumerator Dolly(PlayerController player)
	{
		float t = 0;
		float m = 1 / dollyTime;
		var d = transitionToCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
		bool faded = false;
		while (t < 1)
		{
			t += Time.deltaTime * m;
			d.m_PathPosition = t;
			yield return null;
			if (!faded && t > 0.6f)
			{
				faded = true;
				UIController.singleton.FadeOut(0.1f);
			}
		}
		//Save the player's state temporarily - or maybe transfere go between scenes
		// var stream = new System.IO.MemoryStream();
		// var bw = new System.IO.BinaryWriter(stream);

		// var writer = new GameDataWriter(bw);
		// playerControllerSaveChannel.SaveBin(writer);
		// bw.Flush(); //Do not close the memory stream


		ChangeLevel(player);
	}
	IEnumerator ChangeScene(PlayerController player)
	{
		yield return new WaitForSeconds(dollyTime * 0.6f);
		UIController.singleton.FadeOut(dollyTime * 0.2f);
		yield return new WaitForSeconds(dollyTime * 0.4f);
		ChangeLevel(player);
	}
	void ChangeLevel(PlayerController player)
	{
		player.machine.ChangeToState(player.machine.defaultState);

		//LevelManager.afterLevelLoaded += func;
		LevelManager.LoadLevel(changeToLevel, () => OnLevelLoaded(exitConnection));
	}

	public static void OnLevelLoaded(string exitConnector)
	{

		//Teleport player to exit connector
		foreach (var connector in FindObjectsOfType<SceneConnector>())
		{
			if (connector.connectorIdentifier == exitConnector)
			{
				//Telport here
				Character.playerCharacter.transform.SetPositionAndRotation(connector.exitTransform.position, connector.exitTransform.rotation);
				break;
			}
		}
	}

}