using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class SaveUI : UIMenu
{
    public RectTransform savesMenu;
    public GameObject saveDispayPrefab;

    public Button loadButton;


    protected override void Start()
    {
        base.Start();
        //Add listeners
        loadButton.onClick.AddListener(OnLoad);
    }
    public override void CloseMenu()
    {
        for (int i = 0; i < savesMenu.childCount; i++)
        {
            Destroy(savesMenu.GetChild(i).gameObject);
        }
        base.CloseMenu();
    }
    public void OnLoad()
    {

    }
    public override void OpenMenu()
    {
        base.OpenMenu();
        string rootDir = Application.persistentDataPath + "/saves/save1";
        if (Directory.Exists(rootDir))
        {
            string[] dirs = Directory.GetDirectories(rootDir);
            for (int i = dirs.Length - 1; i >= 0; i--)
            {
                Instantiate(saveDispayPrefab, savesMenu).GetComponent<SaveDisplayUI>().Init(dirs[i]);
            }
        }
    }

}
