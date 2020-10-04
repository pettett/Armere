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

public class SaveManager : MonoBehaviour
{
    public InputAction quicksaveAction = new InputAction("<keyboard>/f5");

    public static string saveName = "save1";

    public const string save = "/save.save";
    public const string metaSave = "/save.metasave";

    public static string SaveDirectory => string.Format("{0}/saves/{1}/{2}",
        Application.persistentDataPath,
        saveName,
        System.DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss")
        );

    public static SaveManager singleton;
    public string lastSaveDir;
    float lastSave;

    public float minAutosaveGap = 4 * 60;

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




    [System.Serializable]
    public class SaveState
    {
        LevelName level;
        InventoryController.InventorySave inventory;
        Armere.PlayerController.PlayerController.PlayerSaveData player;
        NPCManager.NPCSaveData npc;
        public void GatherSaveData()
        {
            level = LevelController.currentLevel;
            inventory = InventoryController.singleton.CreateSave();
            npc = NPCManager.singleton.data;
            player = Armere.PlayerController.PlayerController.activePlayerController.CreateSaveData();
        }

        public void RestoreGameState()
        {
            LevelController.ChangeToLevel(level, OnSceneLoaded);

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            //attempt to retore all the saveables
            print("Loaded save");
            Items.OnSceneChange();

            InventoryController.singleton.RestoreSave(inventory);
            Armere.PlayerController.PlayerController.activePlayerController.RestoreSave(player);
            NPCManager.singleton.data = npc;

            Time.timeScale = 1;
        }
    }

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
            //Save the name of the current region for more context for the player
            regionName = LevelInfo.currentLevelInfo.currentRegionName;
        }
    }

    [System.NonSerialized] public SaveState currentSaveState = null;

    private void Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;
        DontDestroyOnLoad(this);

        lastSave = Time.realtimeSinceStartup - 5;
        quicksaveAction.performed += OnQuicksave;
        quicksaveAction.Enable();
        print("Creating first save state");
        currentSaveState = new SaveState();
    }

    public void OnQuicksave(InputAction.CallbackContext c)
    {
        SaveGameStateAsync();
    }


    public async void AttemptAutoSave()
    {

        if (lastSave + minAutosaveGap < Time.realtimeSinceStartup)
        {
            //Save the game and show an indicator so show the game is saving
            await SaveGameStateAsync();
            print("Autosaved");
            lastSave = Time.realtimeSinceStartup;
        }
        else
        {
            print("Too soon to autosave again");
        }
    }


    public void LoadSave(string dir)
    {
        IFormatter formatter = SaveFormatting.SetupFormatter();
        using (FileStream saveInfoStream = new FileStream(dir + save, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            currentSaveState = (SaveState)formatter.Deserialize(saveInfoStream);
        }
        currentSaveState.RestoreGameState();
    }

    public async void LoadSaveAsync(string dir)
    {
        IFormatter formatter = SaveFormatting.SetupFormatter();
        using (FileStream saveInfoStream = new FileStream(dir + save, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (MemoryStream saveInMemory = new MemoryStream())
            {
                await saveInfoStream.CopyToAsync(saveInMemory);
                currentSaveState = (SaveState)formatter.Deserialize(saveInMemory);
            }
        }
        currentSaveState.RestoreGameState();
    }


    public async Task SaveGameStateAsync()
    {

        //dont allow saving more then once every 5 seconds
        if (lastSave > Time.realtimeSinceStartup - 5)
            return;
        lastSave = Time.realtimeSinceStartup;

        //setup directory
        string dir = SaveDirectory;
        System.IO.Directory.CreateDirectory(dir);
        //Debug.LogFormat("Quick saving to {0}", dir);

        lastSaveDir = dir;


        IFormatter formatter = SaveFormatting.SetupFormatter();

        //TODO - do this automatically
        currentSaveState.GatherSaveData();


        using (MemoryStream memoryStream = new MemoryStream())
        {
            //Current save state is updated during the game, so it can be stored raw
            formatter.Serialize(memoryStream, currentSaveState);
            using (Stream stream = new FileStream(dir + save, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                //Copy into storage in the background
                await memoryStream.CopyToAsync(stream);
            }
        }

        SaveInfo info = new SaveInfo(ScreenshotCapture.CaptureScreenshot(128, 128));

        info.saveTime = System.DateTime.Now;


        using (MemoryStream memoryStream = new MemoryStream())
        {
            formatter.Serialize(memoryStream, info);
            using (Stream stream = new FileStream(dir + metaSave, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                //Copy into storage in the background
                await memoryStream.CopyToAsync(stream);
            }
        }

    }


}
