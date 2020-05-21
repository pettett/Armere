using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    public static string Filepath(string filename) => Application.persistentDataPath + "/" + filename;

    public static void SaveData<T>(T config, string filename)
    {
        // 2
        // XmlSerializer bf = new XmlSerializer(typeof(T));
        string path = Filepath(filename);
        // FileStream file = File.Create(path + ".xml");


        File.WriteAllText(path + ".json", JsonUtility.ToJson(config, true));

        //  bf.Serialize(file, config);
        //  file.Close();
    }

    public static T LoadData<T>(string filename) where T : new()
    {
        string p = Filepath(filename) + ".json";
        if (File.Exists(p))
            return JsonUtility.FromJson<T>(File.ReadAllText(p));
        else
        {
            T t = new T();
            SaveData<T>(t, filename);
            return t;
        }
    }
}
