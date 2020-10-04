using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public static LevelInfo currentLevelInfo;
    public LevelName levelName;
    public GameObject player;
    public Transform playerTransform => player.transform;

    public string currentRegionName;

    private void Awake()
    {
        currentLevelInfo = this;
        LevelController.currentLevel = levelName;
    }
}
