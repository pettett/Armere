using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
	void SaveBin(in GameDataWriter writer);
	void LoadBin(in GameDataReader reader);
}
public abstract class MonoSaveable : MonoBehaviour, ISaveable
{
	public abstract void SaveBin(in GameDataWriter writer);
	public abstract void LoadBin(in GameDataReader reader);
	public virtual void LoadBlank()
	{

	}
}

