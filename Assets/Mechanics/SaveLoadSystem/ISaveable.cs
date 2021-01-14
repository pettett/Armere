using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    void SaveBin(GameDataWriter writer);
    void LoadBin(Version saveVersion, GameDataReader reader);
}
public abstract class MonoSaveable : MonoBehaviour, ISaveable
{
    public abstract void SaveBin(GameDataWriter writer);
    public abstract void LoadBin(Version saveVersion, GameDataReader reader);
    public virtual void LoadBlank()
    {

    }
}

