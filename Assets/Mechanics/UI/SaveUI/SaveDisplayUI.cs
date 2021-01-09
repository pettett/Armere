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
    int saveIndex;

    public string AdaptiveTime(System.DateTime time)
    {
        System.DateTime now = System.DateTime.Now;
        if (now.Year != time.Year) return time.ToString("H:mm, dd/MM/yyyy");
        if (now.Month != time.Month || now.Day != time.Day) return time.ToString("H:mm, dd/MM");

        if (now.Minute != time.Minute || now.Hour != time.Hour) return time.ToString("H:mm,") + " Today";
        return "Just Now";
    }

    public void Init(int saveIndex)
    {
        string dir = SaveManager.GetDirectoryForSaveInstance(saveIndex);

        IFormatter formatter = new BinaryFormatter();

        using (Stream saveInfoStream = new FileStream(Path.Combine(dir, SaveManager.metaSaveRecordFileName), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            SaveManager.SaveInfo saveInfo = (SaveManager.SaveInfo)formatter.Deserialize(saveInfoStream);

            scene.text = saveInfo.regionName;
            saveTime.text = AdaptiveTime(saveInfo.saveTime);

            Texture2D tex = new Texture2D(128, 128);
            tex.LoadImage(saveInfo.thumbnail);
            thumbnail.texture = tex;

        }



        this.saveIndex = saveIndex;
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }
    void OnClicked()
    {
        SaveManager.singleton.LoadSave(saveIndex, true);
    }
}
