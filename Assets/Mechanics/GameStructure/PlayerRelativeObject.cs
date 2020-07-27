

using UnityEngine;
using System.Collections.Generic;

public class PlayerRelativeObject : MonoBehaviour
{
    public static List<PlayerRelativeObject> relativeObjects = new List<PlayerRelativeObject>();
    public float enableRange = 50;
    public float disableRange = 60;
    protected void AddToRegister()
    {
        relativeObjects.Add(this);
    }
    protected void RemoveFromRegister()
    {
        relativeObjects.Remove(this);
    }
    public virtual void OnPlayerInRange() { enabled = true; }
    public virtual void OnPlayerOutRange() { enabled = false; }
}

