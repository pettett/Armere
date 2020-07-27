using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ItemPropertyBase : ScriptableObject
{
    public virtual void CreatePlayerObject(GameObject mesh)
    {

    }
    public virtual void OnItemEquip(Animator playerAnim)
    {

    }
    public virtual void OnItemDeEquip(Animator playerAnim)
    {

    }
}