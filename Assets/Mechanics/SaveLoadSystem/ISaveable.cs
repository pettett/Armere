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
    public virtual void SaveBin(GameDataWriter writer)
    {

    }
    public virtual void LoadBin(Version saveVersion, GameDataReader reader)
    {

    }
    public virtual void LoadBlank()
    {

    }
}

