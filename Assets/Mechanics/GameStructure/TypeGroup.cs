
using System.Collections.Generic;
using UnityEngine;

public interface IScanable
{
    Transform transform { get; }
    Vector3 offset { get; }
    bool enabled{ get; }
}


public static class TypeGroup<T> where T : IScanable
{
    public static List<T> allObjects = new List<T>();
}
