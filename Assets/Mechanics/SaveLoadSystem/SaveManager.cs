using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Profiling;




// [System.Serializable]
// public class SaveState
// {

//     [System.Serializable]
//     struct ItemSpawnerInfo
//     {
//         public bool spawned;

//         public ItemSpawnerInfo(ItemSpawner spawner)
//         {
//             this.spawned = spawner.spawnedItem;
//         }
//     }



//     LevelName level;
//     InventoryController.InventorySave inventory;
//     Armere.PlayerController.PlayerController.PlayerSaveData player;
//     NPCManager.NPCSaveData npc;
//     QuestManager.QuestBook quests;

//     Dictionary<System.Guid, ItemSpawnerInfo> itemSpawnerData;



//     public void GatherSaveData()
//     {
//         level = LevelController.currentLevel;
//         inventory = InventoryController.singleton.CreateSave();
//         npc = NPCManager.singleton.data;
//         player = Armere.PlayerController.PlayerController.activePlayerController.CreateSaveData();
//         quests = QuestManager.singleton.questBook;


//         itemSpawnerData = MonoBehaviour.FindObjectsOfType<ItemSpawner>().ToDictionary(
//             sp => sp.GetComponent<GuidComponent>().GetGuid(),
//             sp => new ItemSpawnerInfo(sp));



//     }

//     public void CreateEmptySave()
//     {
//         level = LevelController.currentLevel;
//         inventory = new InventoryController.InventorySave();
//         npc = NPCManager.singleton.data;
//         quests = QuestManager.singleton.questBook;


//         itemSpawnerData = new Dictionary<System.Guid, ItemSpawnerInfo>();
//     }

//     public void RestoreGameState(bool hardLoad)
//     {
//         if (hardLoad)
//             LevelController.ChangeToLevel(level, OnSceneLoaded);
//         else ActivateSaveReceivers();
//     }

//     void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
//     {
//         ActivateSaveReceivers();
//     }
//     public void ActivateSaveReceivers()
//     {
//         Profiler.BeginSample("Restoring Game State");

//         Profiler.BeginSample("Restoring Inventory");
//         InventoryController.singleton.OnSaveStateLoaded(inventory);
//         Profiler.EndSample();
//         Profiler.BeginSample("Restoring Quests");
//         QuestManager.singleton.OnSaveStateLoaded(quests);
//         Profiler.EndSample();
//         Profiler.BeginSample("Restoring NPCs");
//         NPCManager.singleton.data = npc;
//         Profiler.EndSample();


//         if (player.parallels != null)
//         {
//             Profiler.BeginSample("Restoring Player");
//             Armere.PlayerController.PlayerController.activePlayerController.OnSaveStateLoaded(player);
//             Profiler.EndSample();
//         }




//         Profiler.BeginSample("Restoring Item Spawners");
//         //Item spawners need to be found and set
//         foreach (var spawner in MonoBehaviour.FindObjectsOfType<ItemSpawner>())
//         {
//             if (itemSpawnerData.TryGetValue(spawner.GetComponent<GuidComponent>().GetGuid(), out var data))
//             {
//                 //This should be set before the spawner's start value
//                 spawner.spawnedItem = data.spawned;
//             }
//         }
//         Profiler.EndSample();

//         //TODO: Reset all addressable assets and stuff

//         Time.timeScale = 1;
//         Profiler.EndSample();
//     }
// }

public readonly struct Version{
    //Takes up 4 bytes - same as an integer
    public readonly byte major;
    public readonly byte minor;
    public readonly ushort patch;

    public Version(byte major, byte minor, ushort patch)
    {
        this.major = major;
        this.minor = minor;
        this.patch = patch;
    }
    public override string ToString(){
        return string.Join(".", major, minor, patch);
    }
}

public class SaveManager : MonoBehaviour
{

    public static readonly Version version = new Version(0, 0, 1);

    public InputAction quicksaveAction = new InputAction("<keyboard>/f5");

    public static string saveDirectoryName = "save1";
    public const string saveRecordFileName = "save.save";
    public const string metaSaveRecordFileName = "save.metasave";
    public const uint maxSaves = 10;
    public static string NewSaveDirectory() => Path.Combine(Application.persistentDataPath, "saves", saveDirectoryName, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

    public static SaveManager singleton;
    public string lastSaveDir;
    float lastSave;

    public float minAutosaveGap = 4 * 60;

    public bool autoLoadOnStart = true;
    public bool autoSaveOnDestroy = true;

    public SceneSaveData sceneSaveData;
    public MonoSaveable[] saveables;




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



    ///<summary>
    ///Delete all saves.
    ///Maybe not undoable - remember to use with caution
    ///</summary>
    [MyBox.ButtonMethod]
    public void ResetSave()
    {
        string rootDir = Application.persistentDataPath + "/saves/save1";

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
    [MyBox.ButtonMethod]
    public void OpenSaveFolder()
    {
        string rootDir = Application.persistentDataPath + "/saves/save1";
        rootDir = rootDir.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + rootDir);
    }

    //smaller class to hold info about save that is used in the UI
    [System.Serializable]
    public class SaveInfo
    {
        public string regionName;
        public System.DateTime saveTime;
        public byte[] thumbnail;

        public SaveInfo(byte[] thumbnail)
        {
            this.thumbnail = thumbnail;

        }
    }



    private void Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;
        DontDestroyOnLoad(this);

        if (autoLoadOnStart)
            LoadMostRecentSave(false);
        else
            LoadBlankSave();


        lastSave = Time.realtimeSinceStartup - 5;
        quicksaveAction.performed += OnQuicksave;
        quicksaveAction.Enable();
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

    public void OnQuicksave(InputAction.CallbackContext c)
    {
        SaveGameState();
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

    public void LoadBlankSave()
    {
        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i].LoadBlank();
        }
    }

    //Save 0 is the most recent save
    public static string GetDirectoryForSaveInstance(int save)
    {
        string rootDir = Path.Combine(Application.persistentDataPath, "saves/save1");
        //Load the first save in the directory
        var dirs = Directory.GetDirectories(rootDir);

        if (dirs.Length > 0 && save < dirs.Length)
        {
            //Sort and reverse the array so it is in order of top to bottom
            System.Array.Sort(dirs);
            System.Array.Reverse(dirs);

            var last = dirs[save];

            return last;
        }
        else
        {
            //No save at index
            return null;
        }
    }

    public void LoadMostRecentSave(bool hardLoad)
    {
        string dir = GetDirectoryForSaveInstance(0);
        if (dir != null)
            LoadSave(dir, hardLoad);
        else
            LoadBlankSave();
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

    public void LoadSave(string dir, bool hardLoad)
    {
        Profiler.BeginSample("Loading Game");
        IFormatter formatter = SaveFormatting.SetupFormatter();
        using (FileStream saveRecordStream = new FileStream(Path.Combine(dir, saveRecordFileName), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var saveRecord = (object[])formatter.Deserialize(saveRecordStream);

            for (int i = 0; i < saveables.Length; i++)
            {

                saveables[i].Load(saveRecord[i + 1]);
            }

        }
        Profiler.EndSample();




    }


    public void SaveGameState()
    {
        Profiler.BeginSample("Saving Game");

        //dont allow saving more then once every 5 seconds

        lastSave = Time.realtimeSinceStartup;

        //setup directory
        string dir = NewSaveDirectory();
        Directory.CreateDirectory(dir);
        //Debug.LogFormat("Quick saving to {0}", dir);

        lastSaveDir = dir;


        IFormatter formatter = SaveFormatting.SetupFormatter();

        //TODO - do this automatically

        object[] saveData = new object[saveables.Length + 1];
        saveData[0] = sceneSaveData.SaveLevelData();
        for (int i = 0; i < saveables.Length; i++)
        {
            saveData[i + 1] = saveables[i].Save();
        }


        //Current save state is updated during the game, so it can be stored raw

        using (Stream stream = new FileStream(Path.Combine(dir, saveRecordFileName), FileMode.Create, FileAccess.Write, FileShare.None))
        {

            formatter.Serialize(stream, saveData);
        }


        SaveInfo info = new SaveInfo(ScreenshotCapture.CaptureScreenshot(128, 128));

        info.saveTime = System.DateTime.Now;
        info.regionName = sceneSaveData.SaveTooltip;

        using (Stream stream = new FileStream(Path.Combine(dir, metaSaveRecordFileName), FileMode.Create, FileAccess.Write, FileShare.None))
        {
            //Copy into storage in the background
            formatter.Serialize(stream, info);
        }

        Profiler.EndSample();
    }


}
