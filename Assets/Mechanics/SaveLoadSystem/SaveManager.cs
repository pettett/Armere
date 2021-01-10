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



public readonly struct Version
{
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

    public override bool Equals(object obj)
    {
        return obj is Version version &&
               major == version.major &&
               minor == version.minor &&
               patch == version.patch;
    }
    public static bool operator ==(Version lhs, Version rhs)
    {
        return lhs.patch == rhs.patch && lhs.minor == rhs.minor && lhs.major == rhs.major;
    }
    public static bool operator !=(Version lhs, Version rhs) => !(lhs == rhs);


    public override int GetHashCode()
    {
        return major << 24 + minor << 16 + patch;
    }

    public override string ToString()
    {
        return string.Join(".", major, minor, patch);
    }
}
public readonly struct GameDataReader
{
    public readonly BinaryReader reader;

    public GameDataReader(BinaryReader reader)
    {
        this.reader = reader;
    }

    public int ReadInt() => reader.ReadInt32();
    public uint ReadUInt() => reader.ReadUInt32();
    public bool ReadBool() => reader.ReadBoolean();
    public float ReadFloat() => reader.ReadSingle();
    public string ReadString() => reader.ReadString();
    public char ReadChar() => reader.ReadChar();
    public long ReadLong() => reader.ReadInt64();

    public System.Guid ReadGuid() => new System.Guid(ReadBytes(16));
    public byte[] ReadBytes(int count) => reader.ReadBytes(count);

    public Version ReadVersion() => new Version(reader.ReadByte(), reader.ReadByte(), reader.ReadUInt16());
    public Vector3 ReadVector3() => new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    public Vector2 ReadVector2() => new Vector2(ReadFloat(), ReadFloat());
    public Quaternion ReadQuaternion() => new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());

    //Read List functions read a list using metadata also stored
    public byte[] ReadListByte()
    {
        return ReadBytes(ReadInt());
    }

}
public readonly struct GameDataWriter
{
    public readonly BinaryWriter writer;

    public GameDataWriter(BinaryWriter writer)
    {
        this.writer = writer;
    }

    public void Write(int value) => writer.Write(value);
    public void Write(long value) => writer.Write(value);
    public void Write(bool value) => writer.Write(value);
    public void Write(float value) => writer.Write(value);
    public void Write(uint value) => writer.Write(value);
    public void Write(char value) => writer.Write(value);
    public void Write(byte[] value) => writer.Write(value);
    public void Write(System.Guid value) => writer.Write(value.ToByteArray());
    public void Write(Version value)
    {
        writer.Write(value.major);
        writer.Write(value.minor);
        writer.Write(value.patch);
    }
    public void Write(Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }
    public void Write(Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
    public void Write(Vector2 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
    }
    public void Write(string value) => writer.Write(value);


    //Write list functions store some metadata about the list so it can be easily loaded
    public void WriteList(byte[] byteList)
    {
        Write(byteList.Length);
        Write(byteList);
    }

}
public class SaveManager : MonoBehaviour
{

    public static readonly Version version = new Version(0, 0, 1);

    public InputAction quicksaveAction = new InputAction("<keyboard>/f5");
    public const string savesDirectoryName = "saves";
    public static string currentSaveFileDirectoryName = "save1";
    public const string saveRecordFileName = "save.binsave";
    public const string metaSaveRecordFileName = "save.metasave";
    public const uint maxSaves = 10;
    public static string NewSaveDirectory() => Path.Combine(Application.persistentDataPath, savesDirectoryName, currentSaveFileDirectoryName, System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));

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



    //smaller class to hold info about save that is used in the UI
    [System.Serializable]
    public class SaveInfo : ISaveable
    {
        //16 by 9 ratio
        public const int thumbnailWidth = 8 * 16;
        public const int thumbnailHeight = 8 * 10;
        public string regionName;
        public System.DateTime saveTime;
        public Texture2D thumbnail;

        public SaveInfo(Version saveVersion, GameDataReader reader)
        {
            LoadBin(saveVersion, reader);
        }
        public SaveInfo(string regionName)
        {
            this.thumbnail = ScreenshotCapture.CaptureScreenshot(thumbnailWidth, thumbnailHeight);
            this.regionName = regionName;
            saveTime = System.DateTime.Now;
        }

        public void LoadBin(Version saveVersion, GameDataReader reader)
        {

            thumbnail = new Texture2D(thumbnailWidth, thumbnailHeight);
            thumbnail.LoadImage(reader.ReadListByte());

            saveTime = System.DateTime.FromFileTime(reader.ReadLong());

            regionName = reader.ReadString();
        }

        public void SaveBin(GameDataWriter writer)
        {

            var thumb = thumbnail.EncodeToPNG();

            writer.WriteList(thumb);


            writer.Write(saveTime.ToFileTime());

            writer.Write(regionName);
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
        Debug.Log("Loading blank save...");
        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i].LoadBlank();
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
        Debug.Log("Loading Game...");

        string savePath = Path.Combine(dir, saveRecordFileName);

        using (var reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        {
            GameDataReader gameDataReader = new GameDataReader(reader);
            //Save the version of the game saved
            var version = gameDataReader.ReadVersion();
            for (int i = 0; i < saveables.Length; i++)
            {
                Debug.Log($"Loading {i}");
                saveables[i].LoadBin(version, gameDataReader);
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

        string savePath = Path.Combine(dir, saveRecordFileName);

        WriteFile(savePath, saveables);

        SaveInfo info = new SaveInfo(sceneSaveData.SaveTooltip);

        WriteFile(Path.Combine(dir, metaSaveRecordFileName), info);

        Profiler.EndSample();
    }

    public static void WriteFile(string dir, params ISaveable[] saveables)
    {
        using (var writer = new BinaryWriter(File.Open(dir, FileMode.Create)))
        {
            GameDataWriter gameWriter = new GameDataWriter(writer);
            //Save the version of the game saved
            gameWriter.Write(version);
            for (int i = 0; i < saveables.Length; i++)
            {
                saveables[i].SaveBin(gameWriter);
            }
        }
    }

    public static SaveInfo LoadSaveInfo(int saveIndex) => LoadSaveInfo(GetDirectoryForSaveInstance(saveIndex));

    public static SaveInfo LoadSaveInfo(string saveDirectory)
    {

        SaveInfo info = null;
        using (var reader = new BinaryReader(File.Open(Path.Combine(saveDirectory, metaSaveRecordFileName), FileMode.Open)))
        {
            GameDataReader gameDataReader = new GameDataReader(reader);
            //Save the version of the game saved
            var version = gameDataReader.ReadVersion();
            info = new SaveInfo(version, gameDataReader);
        }
        return info;
    }

}
