using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public LevelName levelName;

    private void Start()
    {
        LevelController.currentLevel = levelName;
    }
}
