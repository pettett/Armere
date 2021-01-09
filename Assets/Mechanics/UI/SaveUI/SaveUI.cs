using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class SaveUI : UIMenu
{
    public Button confirm;
    public TextMeshProUGUI text;
    public string saveText = "Are you sure you would like to save?";
    public string savingText = "Saving...";
    public string savedText = "Saved";
    protected override void Start()
    {
        confirm.onClick.AddListener(OnConfirm);
        base.Start();
    }

    public override void OpenMenu()
    {
        base.OpenMenu();
        backButton.gameObject.SetActive(true);
        confirm.gameObject.SetActive(true);
        text.text = saveText;
    }

    async void OnConfirm()
    {
        backButton.gameObject.SetActive(false);
        confirm.gameObject.SetActive(false);
        text.text = savingText;

        SaveManager.singleton.SaveGameState();

        text.text = savedText;
        await Task.Delay(100);
        CloseMenu();
    }




}
