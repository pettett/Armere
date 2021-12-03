using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;
using UnityEngine.Profiling;
using System.Collections;

public interface IBinaryVariableWritableSerializer<T>
{
	void Write(in GameDataWriter writer);
}

public interface IBinaryVariableSerializer<T> : IBinaryVariableWritableSerializer<T>
{
	T Read(in GameDataReader reader);
}

public interface IBinaryVariableAsyncSerializer<T> : IBinaryVariableWritableSerializer<T>
{
	void Read(in GameDataReader reader, System.Action<T> data);
}


enum PrimitiveCode : byte
{
	Unsigned = 0b1000_0000,
	Long = 1,
	Short = 2,
	Byte = 3,
	Bool = 4,
	Float = 5,
	Char = 6,
	Vector3 = 7,
	Vector2 = 8,
	Quaternion = 9,
	Int = 10,
	UInt = Unsigned | Int,
	ULong = Unsigned | Long,
	UShort = Unsigned | Short,
}
public class SaveManager : MonoBehaviour
{

	public static readonly Version version = new Version(0, 0, 6);

	public const string savesDirectoryName = "saves";
	public static string currentSaveFileDirectoryName = "save1";
	public const string saveRecordFileName = "save.binsave";
	public const string metaSaveRecordFileName = "save.metasave";
	public const int maxSaves = 10;
	public static string SaveRootDirectory => Path.Combine(Application.persistentDataPath, savesDirectoryName, currentSaveFileDirectoryName);
	public static string NewSaveDirectory => Path.Combine(Application.persistentDataPath, savesDirectoryName, currentSaveFileDirectoryName, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

	public static SaveManager singleton;
	public string lastSaveDir;
	float lastSave;

	public float minAutosaveGap = 4 * 60;

	public bool autoLoadOnStart = true;
	public bool autoSaveOnDestroy = true;

	public SceneSaveData sceneSaveData;
	public SaveableSO[] saveLoadEventChannels;

	public UnityEngine.Events.UnityEvent OnBlankSaveLoaded;
	public static event System.Action OnGameLoadingCompleted;
	public static bool gameLoadingCompleted;

	/*
	Save Structure:
	Saves
		-Save1
			-27-06-2020-11-41-40
				-save.save
				-save.metasave
			etc up to limit
		-Save2
			same structure
		etc.
	*/
	[Header("Event Channels")]
	public InputReader input;
	public VoidEventChannelSO onSavingBegin;
	public VoidEventChannelSO onSavingFinish;

	///<summary>
	///Delete all saves.
	///Maybe not undoable - remember to use with caution
	///</summary>
	[MyBox.ButtonMethod]
	public void ResetSave()
	{
		string rootDir = SaveRootDirectory;

		if (Directory.Exists(rootDir))
		{
			print("Deleting all saves");
			DirectoryInfo info = new DirectoryInfo(rootDir);

			foreach (var dir in info.GetDirectories())
			{
				//delete recursively as each directory has 2 files
				dir.Delete(true);
			}
		}
	}



	//smaller class to hold info about save that is used in the UI
	[System.Serializable]
	public class SaveInfo
	{
		//16 by 9 ratio
		public const int thumbnailWidth = 8 * 16;
		public const int thumbnailHeight = 8 * 10;
		public string regionName;
		public System.DateTime saveTime;
		public Texture2D thumbnail;

		public SaveInfo(in GameDataReader reader)
		{
			LoadBin(reader);
		}
		public SaveInfo(string regionName)
		{
			this.thumbnail = ScreenshotCapture.CaptureScreenshot(thumbnailWidth, thumbnailHeight);
			this.regionName = regionName;
			saveTime = System.DateTime.Now;
		}

		public void LoadBin(in GameDataReader reader)
		{

			thumbnail = new Texture2D(thumbnailWidth, thumbnailHeight);
			thumbnail.LoadImage(reader.ReadListByte());

			saveTime = System.DateTime.FromFileTime(reader.ReadLong());

			regionName = reader.ReadString();
		}

		public void SaveBin(in GameDataWriter writer)
		{

			var thumb = thumbnail.EncodeToPNG();

			writer.WriteList(thumb);


			writer.WritePrimitive(saveTime.ToFileTime());

			writer.WritePrimitive(regionName);
		}

		public string AdaptiveTime()
		{
			System.DateTime now = System.DateTime.Now;
			if (now.Year != saveTime.Year) return saveTime.ToString("H:mm, dd/MM/yyyy");
			if (now.Month != saveTime.Month || now.Day != saveTime.Day) return saveTime.ToString("H:mm, dd/MM");

			if (now.Minute != saveTime.Minute || now.Hour != saveTime.Hour) return saveTime.ToString("H:mm,") + " Today";
			return "Just Now";
		}
	}



	private void Start()
	{
		if (singleton != null)
		{
			Destroy(gameObject);
			return;
		}
		Debug.Log("New save singleton");
		singleton = this;

		// if (autoLoadOnStart)
		// 	LoadMostRecentSave(false);
		// else
		// 	SoftLoadBlankSave();


		lastSave = Time.realtimeSinceStartup - 5;

		input.quicksaveEvent += OnQuicksave;
		input.quickloadEvent += OnQuickload;

	}

	//"do not destroy on load"
	private void OnApplicationQuit()
	{
		if (autoSaveOnDestroy)
		{
			Debug.Log("Saving game");
			SaveGameState();
		}
	}

	public void OnQuicksave(InputActionPhase phase)
	{
		if (phase == InputActionPhase.Performed)
			SaveGameState();
	}
	public void OnQuickload(InputActionPhase phase)
	{
		if (phase == InputActionPhase.Performed)
			LoadMostRecentSave(true);
	}

	public void AttemptAutoSave()
	{
		if (lastSave + minAutosaveGap < Time.realtimeSinceStartup)
		{
			//Save the game and show an indicator so show the game is saving
			SaveGameState();
			print("Autosaved");
			lastSave = Time.realtimeSinceStartup;
		}
		else
		{
			print("Too soon to autosave again");
		}
	}



	//Save 0 is the most recent save
	public static string GetDirectoryForSaveInstance(int save)
	{
		string rootDir = Path.Combine(Application.persistentDataPath, savesDirectoryName, currentSaveFileDirectoryName);
		//Load the first save in the directory
		var saveDirectories = SaveFileSaveDirectories().ToArray();

		if (saveDirectories.Length > 0 && save < saveDirectories.Length)
		{
			//Sort and reverse the array so it is in order of top to bottom
			var last = saveDirectories[save];

			return last;
		}
		else
		{
			//No save at index
			return null;
		}
	}
	public static IEnumerable<string> SaveFileSaveDirectories()
	{
		string selectedSaveFileDirectory = Path.Combine(Application.persistentDataPath, savesDirectoryName, currentSaveFileDirectoryName);

		if (Directory.Exists(selectedSaveFileDirectory))
		{
			string[] dirs = Directory.GetDirectories(selectedSaveFileDirectory);

			if (dirs.Length == 0) yield break;

			System.Array.Sort(dirs);
			System.Array.Reverse(dirs);

			for (int i = 0; i < dirs.Length; i++)
			{
				//Valid save file needs a binsave file and a metasave file
				if (File.Exists(Path.Combine(dirs[i], metaSaveRecordFileName)) && File.Exists(Path.Combine(dirs[i], saveRecordFileName)))
					yield return dirs[i];
			}
		}
	}

	public static int GetSaveCount() => SaveFileSaveDirectories().Count();

	public void LoadMostRecentSave(bool hardLoad)
	{
		string dir = GetDirectoryForSaveInstance(0);
		if (dir != null)
			LoadSave(dir, hardLoad);
		else
			LoadBlankSave(hardLoad);
	}

	public void LoadSave(int saveIndex, bool hardLoad)
	{
		string dir = GetDirectoryForSaveInstance(saveIndex);
		if (dir == null)
		{
			throw new System.ArgumentException("Save index outside range of saves");
		}
		LoadSave(dir, hardLoad);
	}


	public void DeleteSave(int saveIndex) => DeleteSave(GetDirectoryForSaveInstance(saveIndex));
	public void DeleteSave(string dir)
	{
		if (dir != null)
			Directory.Delete(dir, true);
	}


	public void LoadSave(string dir, bool hardLoad)
	{
		if (hardLoad)
		{
			HardLoadSave(dir);
		}
		else
		{
			StartCoroutine(SoftLoadSave(dir));
		}
	}

	public void LoadBlankSave(bool hardLoad)
	{
		if (hardLoad)
			HardLoadBlankSave();
		else
			SoftLoadBlankSave();
	}

	public void HardLoadBlankSave()
	{
		//Reload current Scene

		Debug.Log("Hard loading blank save...");
		HardSceneLoad(LevelManager.singleton.currentLevel, () =>
		{
			SoftLoadBlankSave();
		});

	}
	public void SoftLoadBlankSave()
	{
		Debug.Log("Soft loading blank save...");
		for (int i = 0; i < saveLoadEventChannels.Length; i++)
		{
			saveLoadEventChannels[i].LoadBlank();
		}
		OnBlankSaveLoaded?.Invoke();


		OnAfterLoad();
	}

	public void HardLoadSave(string dir)
	{

		string savePath = Path.Combine(dir, saveRecordFileName);

		string levelName;
		using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
		{
			using GameDataReader gameDataReader = new GameDataReader(reader);

			levelName = gameDataReader.ReadString();
		}

		HardSceneLoad(levelName, () =>
		{
			StartCoroutine(SoftLoadSave(dir));
		});
	}
	public void HardSceneLoad(string sceneName, System.Action OnCompleted)
	{

		LevelManager.LoadLevel(sceneName, OnCompleted);


	}

	public IEnumerator SoftLoadSave(string dir)
	{
		Profiler.BeginSample("Loading Game");
		Debug.Log("Loading Game…");


		string savePath = Path.Combine(dir, saveRecordFileName);

		using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
		{
			using GameDataReader gameDataReader = new GameDataReader(reader);

			//Level name not actually used - scene already loaded here

			string levelName = gameDataReader.ReadString();


			for (int i = 0; i < saveLoadEventChannels.Length; i++)
			{
				//Debug.Log($"Loading {i} from position {gameDataReader.reader.BaseStream.Position}");
				gameDataReader.BeginRegion();
				saveLoadEventChannels[i].LoadBin(gameDataReader);
				if (saveLoadEventChannels[i] is LoadableAsyncSO async)
				{
					yield return async.LoadBinAsync(gameDataReader);
				}

				if (!gameDataReader.EndRegion())
				{
					break;
				}

			}

		}

		OnAfterLoad();

		Profiler.EndSample();
	}
	public void OnAfterLoad()
	{
		OnGameLoadingCompleted?.Invoke();
		gameLoadingCompleted = true;
		foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
		{
			if (go && go.transform.parent == null)
			{
				go.gameObject.BroadcastMessage("OnAfterGameLoaded", SendMessageOptions.DontRequireReceiver);
			}
		}
		Debug.Log("Finished load");
	}


	public void SaveGameState()
	{
		Profiler.BeginSample("Saving Game");
		onSavingBegin.RaiseEvent();
		Debug.Log($"Saving Game in {LevelManager.singleton.currentLevel}");

		//dont allow saving more then once every 5 seconds

		lastSave = Time.realtimeSinceStartup;

		//setup directory
		string dir = NewSaveDirectory;
		Directory.CreateDirectory(dir);
		//Debug.LogFormat("Quick saving to {0}", dir);

		lastSaveDir = dir;

		string savePath = Path.Combine(dir, saveRecordFileName);

		//Save the actual file

		WriteFile(savePath, gameWriter =>
		{
			gameWriter.WritePrimitive(LevelManager.singleton.currentLevel);
			for (int i = 0; i < saveLoadEventChannels.Length; i++)
			{
				gameWriter.BeginRegion();
				saveLoadEventChannels[i].SaveBin(gameWriter);
				gameWriter.EndRegion();
			}
		});

		SaveInfo info = new SaveInfo(sceneSaveData.SaveTooltip);

		WriteFile(Path.Combine(dir, metaSaveRecordFileName), gameWriter =>
		{
			info.SaveBin(gameWriter);
		});

		//If there are now more than max saves, remove the last save 
		while (GetSaveCount() > maxSaves)
		{
			DeleteSave(maxSaves);
		}

		onSavingFinish.RaiseEvent();
		Profiler.EndSample();
	}

	public static void WriteFile(string dir, System.Action<GameDataWriter> saveFunction)
	{
		using (var writer = new BinaryWriter(File.Open(dir, FileMode.Create)))
		{
			using GameDataWriter gameWriter = new GameDataWriter(writer);

			saveFunction(gameWriter);

		}
	}

	public static SaveInfo LoadSaveInfo(int saveIndex) => LoadSaveInfo(GetDirectoryForSaveInstance(saveIndex));

	public static SaveInfo LoadSaveInfo(string saveDirectory)
	{

		SaveInfo info;
		using (var reader = new BinaryReader(File.Open(Path.Combine(saveDirectory, metaSaveRecordFileName), FileMode.Open)))
		{
			using GameDataReader gameDataReader = new GameDataReader(reader);
			info = new SaveInfo(gameDataReader);
		}
		return info;
	}

}
