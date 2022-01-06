using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Armere.PlayerController;
using System.IO;
using Armere.Inventory;
/// <summary>
/// Class exists to initialize scenes, with full knowledge of everything that exists
/// 
/// Dependency injector
/// </summary>
public class GameConstructor : MonoBehaviour
{

	//Save order:
	// - Inventory
	// - Quests
	// - NPCs
	// - Spawned items
	// - Player
	// - Stats



	[SerializeField] private PlayerController playerPrefab;

	[Header("Saving")]
	public InventoryController inventory;
	public QuestManager questManager;
	public NPCManager nps;
	public SpawnedItemSaver spawnedItemSaver;

	private PlayerController player;

	public Statistics statistics;
	[Header("Loading")]
	public VoidEventChannelSO loadBlankSave;


	[Header("Channels")]
	public VoidEventChannelSO triggerSave;
	public VoidEventChannelSO triggerAutosave;
	public IntEventChannelSO triggerLoadSave;
	public VoidEventChannelSO triggerLoadMostRecent;

	public VoidEventChannelSO onSavingBegin;
	public VoidEventChannelSO onSavingFinish;


	private void Start()
	{
		Assert.IsNotNull(inventory);
		Assert.IsNotNull(questManager);
		Assert.IsNotNull(nps);
		Assert.IsNotNull(spawnedItemSaver);
		Assert.IsNotNull(statistics);

		LoadNewGame();
	}
	string dataFile = "data";


	// Start is called before the first frame update
	void OnEnable()
	{
		triggerSave.OnEventRaised += Save;
		triggerAutosave.OnEventRaised += Save;


		triggerLoadMostRecent.OnEventRaised += LoadMostRecent;
		triggerLoadSave.OnEventRaised += LoadSave;
	}
	private void OnDisable()
	{
		triggerSave.OnEventRaised -= Save;
		triggerAutosave.OnEventRaised -= Save;

		triggerLoadMostRecent.OnEventRaised -= LoadMostRecent;
		triggerLoadSave.OnEventRaised -= LoadSave;
	}

	void CleanUpOldState()
	{
		if (player != null)
		{
			Debug.Log("Destroying player");
			Destroy(player.gameObject);
		}
	}

	public void LoadNewGame()
	{
		CleanUpOldState();

		player = Instantiate(playerPrefab);

		inventory.RegisterForCommands();

		loadBlankSave?.RaiseEvent();

		foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
		{
			if (go && go.transform.parent == null)
			{
				go.gameObject.BroadcastMessage("OnAfterGameLoaded", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void Save()
	{
		string dir = SaveManager.NewSaveDirectory;

		SaveManager.CreateSaveDirectory(dir);

		Debug.Log("Saving...");

		SaveManager.WriteFromObject(inventory, dir, "inventory");
		SaveManager.WriteFromObject(questManager, dir, "quests");
		SaveManager.WriteFromObject(nps, dir, "npcs");
		SaveManager.WriteFromObject(spawnedItemSaver, dir, "items");
		SaveManager.WriteFromObject(player, dir, "player");
		SaveManager.WriteFromObject(statistics, dir, "stats");
	}

	void LoadMostRecent()
	{
		LoadSave(0);
	}
	public void LoadSave(int index)
	{
		StartCoroutine(LoadSave(SaveManager.GetDirectoryForSaveInstance(index)));
	}
	public IEnumerator LoadSave(string dir)
	{
		CleanUpOldState();

		yield return null;

		player = Instantiate(playerPrefab);

		inventory.RegisterForCommands();

		if (dir != null)
		{

			Debug.Log("Loading...");

			SaveManager.ReadIntoObject(inventory, dir, "inventory", _ =>
			{
				SaveManager.ReadIntoObject(questManager, dir, "quests");

				SaveManager.ReadIntoObject(nps, dir, "npcs");

				SaveManager.ReadIntoObject(spawnedItemSaver, dir, "items");

				SaveManager.ReadIntoObject(player, dir, "player");

				SaveManager.ReadIntoObject(statistics, dir, "stats");
			});
		}

	}


	// Update is called once per frame
	void Update()
	{

	}
}
