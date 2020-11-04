using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Assertions;

[CustomEditor(typeof(WorldObjectData))]
public class WorldObjectDataEditor : Editor
{
    ComponentSettingsListEditor m_ComponentList;

    private void OnEnable()
    {
        m_ComponentList = new ComponentSettingsListEditor(this);
        m_ComponentList.Init(target as WorldObjectData, serializedObject);
    }
    void OnDisable()
    {
        if (m_ComponentList != null)
            m_ComponentList.Clear();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameObject"));
        m_ComponentList.OnGUI();
        serializedObject.ApplyModifiedProperties();
    }

}