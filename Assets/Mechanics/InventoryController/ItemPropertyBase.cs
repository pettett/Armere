﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemPropertyBase : ScriptableObject
{
    static ItemPropertyBase _blank;
    public static ItemPropertyBase blank
    {
        get
        {
            return _blank ?? (_blank = ScriptableObject.CreateInstance<ItemPropertyBase>());
        }
    }

    public virtual GameObject CreatePlayerObject(ItemName name, ItemDatabase db)
    {
        var go = new GameObject(name.ToString(),
            typeof(MeshRenderer),
            typeof(MeshFilter));

        go.GetComponent<MeshFilter>().mesh = db[name].mesh;

        go.GetComponent<MeshRenderer>().materials = db[name].materials;

        return go;
    }
    public virtual void OnItemEquip(Animator playerAnim)
    {

    }
    public virtual void OnItemDeEquip(Animator playerAnim)
    {

    }
}
