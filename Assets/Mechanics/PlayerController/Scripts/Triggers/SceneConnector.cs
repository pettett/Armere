using UnityEngine;
using Cinemachine;
using System.Collections;
using Armere.PlayerController;
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
	public SaveableSO playerControllerSaveChannel;
	public override void OnPlayerTrigger(PlayerController player)
	{
		StartSceneChange(player);
	}

	public void StartSceneChange(PlayerController player)
	{
		transitionToCamera.Priority = 20;
		walker = (AutoWalking)player.ChangeToState(player.autoWalk);
		walker.WalkTo(endTransform.position);
		StartCoroutine(Dolly(player));
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


		player.ChangeToState(player.defaultState);



		SaveableSO test = playerControllerSaveChannel;
		System.Action func = null;
		func = () => OnLevelLoaded(test, null, exitConnection, func);

		//LevelManager.afterLevelLoaded += func;
		LevelManager.LoadLevel(changeToLevel, true, func);

	}

	public static void OnLevelLoaded(SaveableSO playerControllerSaveChannel, System.IO.MemoryStream saveStream, string exitConnector, System.Action func)
	{
		// saveStream.Position = 0;
		// using (var bw = new System.IO.BinaryReader(saveStream))
		// {
		// 	var reader = new GameDataReader(bw);
		// 	playerControllerSaveChannel.LoadBin(reader);
		// }
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

		//LevelController.afterLevelLoaded -= func;
	}

}