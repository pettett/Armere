using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    object Save();
    void Load(object data);
    void LoadBlank();
}

public abstract class MonoSaveable : MonoBehaviour, ISaveable
{
    public abstract object Save();
    public abstract void Load(object data);
    public abstract void LoadBlank();

}

public abstract class MonoSaveable<T> : MonoSaveable
{
    public override object Save() => SaveData();
    public override void Load(object data) => LoadData((T)data);

    public abstract T SaveData();
    public abstract void LoadData(T data);
}
