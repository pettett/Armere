using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : SceneSaveData
{
    public static LevelInfo currentLevelInfo;
    public LevelName levelName;
    public GameObject player;
    public Transform playerTransform => player.transform;

    public override string SaveTooltip => currentRegionName;

    public string currentRegionName;

    private void Awake()
    {
        currentLevelInfo = this;
        LevelController.currentLevel = levelName;
    }

    public override object SaveLevelData()
    {
        return levelName;
    }

    public override string LoadLevelData(object data)
    {
        return ((LevelName)data).ToString();
    }

}
