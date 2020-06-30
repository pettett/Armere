using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

public class SaveDisplayUI : MonoBehaviour
{
    public RawImage thumbnail;
    public TextMeshProUGUI scene;
    public TextMeshProUGUI saveTime;
    string dir;

    public string AdaptiveTime(System.DateTime time)
    {
        System.DateTime now = System.DateTime.Now;
        if (now.Year != time.Year) return time.ToString("H:mm, dd/MM/yyyy");
        if (now.Month != time.Month || now.Day != time.Day) return time.ToString("H:mm, dd/MM");

        if (now.Minute != time.Minute || now.Hour != time.Hour) return time.ToString("H:mm,") + " Today";
        return "Just Now";
    }

    public void Init(string dir)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream saveInfoStream = new FileStream(dir + SaveManager.metaSave, FileMode.Open, FileAccess.Read, FileShare.Read);
        SaveManager.SaveInfo saveInfo = (SaveManager.SaveInfo)formatter.Deserialize(saveInfoStream);
        saveInfoStream.Close();

        scene.text = saveInfo.regionName;
        saveTime.text = AdaptiveTime(saveInfo.saveTime);

        Texture2D tex = new Texture2D(128, 128);
        tex.LoadImage(saveInfo.thumbnail);
        thumbnail.texture = tex;
        this.dir = dir;
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }
    void OnClicked()
    {
        SaveManager.singleton.LoadSave(dir);
    }
}
