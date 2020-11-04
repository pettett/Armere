using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldObjectComponent : MonoBehaviour
{
    public WorldObject worldObject;
    public virtual void SetSettings(WorldObjectDataComponentSettings settings)
    {

    }
}


public abstract class WorldObjectComponent<T> : WorldObjectComponent where T : WorldObjectDataComponentSettings
{
    public T settings;
    public override void SetSettings(WorldObjectDataComponentSettings settings)
    {
        this.settings = (T)settings;
    }
}
