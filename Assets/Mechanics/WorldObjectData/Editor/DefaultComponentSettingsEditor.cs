using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

/// <summary>
/// A default effect editor that gathers all parameters and list them vertically in the
/// inspector.
/// </summary>
public class DefaultComponentSettingsEditor : ComponentSettingsBaseEditor
{
    List<SerializedProperty> m_Parameters;

    /// <summary>
    /// Called when the editor is initialized.
    /// </summary>
    public override void OnEnable()
    {
        m_Parameters = new List<SerializedProperty>();

        var fields = target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t =>
                (t.IsPublic && t.GetCustomAttributes(typeof(NonSerializedAttribute), false).Length == 0)
                || (t.GetCustomAttributes(typeof(UnityEngine.SerializeField), false).Length > 0)
            )
            .ToList();

        foreach (var field in fields)
        {
            var property = serializedObject.FindProperty(field.Name);

            m_Parameters.Add(property);
        }
    }

    /// <summary>
    /// Called every time the inspector is being redrawn. This is where you should add your UI
    /// drawing code.
    /// </summary>
    public override void OnInspectorGUI()
    {
        foreach (var parameter in m_Parameters)
            EditorGUILayout.PropertyField(parameter);
    }
}
