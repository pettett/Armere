using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneSaveData : MonoBehaviour
{
    public abstract string SaveTooltip { get; }
    public abstract object SaveLevelData();
    public abstract string LoadLevelData(object data);

}
