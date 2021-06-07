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



	public void Init(int saveIndex)
	{
		SaveManager.SaveInfo saveInfo = SaveManager.LoadSaveInfo(saveIndex);

		scene.text = saveInfo.regionName;
		saveTime.text = saveInfo.AdaptiveTime();
		thumbnail.texture = saveInfo.thumbnail;

		this.saveIndex = saveIndex;
		GetComponent<Button>().onClick.AddListener(OnClicked);
	}
	void OnClicked()
	{
		SaveManager.singleton.LoadSave(saveIndex, true);
	}
}
