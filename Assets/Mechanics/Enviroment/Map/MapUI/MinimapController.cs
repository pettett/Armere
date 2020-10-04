﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MinimapController : MapUI
{

    protected override void Update()
    {
        Vector3 trackPos = LevelInfo.currentLevelInfo.playerTransform.position;
        map.anchoredPosition = new Vector2(trackPos.x, trackPos.z) * mapScale;
        base.Update();
    }
}
