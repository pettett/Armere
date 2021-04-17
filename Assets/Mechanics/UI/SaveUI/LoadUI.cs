using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class LoadUI : UIMenu
{
	public RectTransform savesMenu;
	public GameObject saveDispayPrefab;

	protected override void Start()
	{
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
				Instantiate(saveDispayPrefab, savesMenu).GetComponent<SaveDisplayUI>().Init(i);
			}
		}
	}

}
