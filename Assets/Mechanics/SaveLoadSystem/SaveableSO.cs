using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SaveableSO : ScriptableObject
{
	public abstract void SaveBin(in GameDataWriter writer);
	public abstract void LoadBin(in GameDataReader reader);
	public abstract void LoadBlank();
}
