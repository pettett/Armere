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

	IntEventChannelSO loadSaveIndex;

	public void Init(int saveIndex, IntEventChannelSO loadSaveIndex)
	{
		SaveManager.SaveInfo saveInfo = SaveManager.LoadSaveInfo(saveIndex);

		scene.text = saveInfo.regionName;
		saveTime.text = saveInfo.AdaptiveTime();
		thumbnail.texture = saveInfo.thumbnail;
		this.loadSaveIndex = loadSaveIndex;

		this.saveIndex = saveIndex;
		GetComponent<Button>().onClick.AddListener(OnClicked);
	}
	void OnClicked()
	{
		loadSaveIndex.RaiseEvent(saveIndex);
	}
}
