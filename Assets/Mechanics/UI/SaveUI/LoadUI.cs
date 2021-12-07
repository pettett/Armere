using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Assertions;
public class LoadUI : UIMenu
{
	public RectTransform savesMenu;
	public GameObject saveDispayPrefab;
	public IntEventChannelSO loadSaveIndex;
	protected override void Start()
	{
		Assert.IsNotNull(loadSaveIndex);

		base.Start();
	}
	public override void CloseMenu()
	{
		for (int i = 0; i < savesMenu.childCount; i++)
		{
			Destroy(savesMenu.GetChild(i).gameObject);
		}
		base.CloseMenu();
	}
	public override void OpenMenu()
	{
		base.OpenMenu();
		string rootDir = SaveManager.SaveRootDirectory;

		if (Directory.Exists(rootDir))
		{
			int saveCount = SaveManager.GetSaveCount();

			for (int i = 0; i < saveCount; i++)
			{
				Instantiate(saveDispayPrefab, savesMenu).GetComponent<SaveDisplayUI>().Init(i, loadSaveIndex);
			}
		}
	}

}
