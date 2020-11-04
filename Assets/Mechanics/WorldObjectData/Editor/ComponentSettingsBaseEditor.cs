using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// The base class for all post-processing effect related editors. If you want to customize the
/// look of a custom post-processing effect, inherit from <see cref="PostProcessEffectEditor{T}"/>
/// instead.
/// </summary>
/// <seealso cref="PostProcessEffectEditor{T}"/>
public class ComponentSettingsBaseEditor
{
    internal WorldObjectDataComponentSettings target { get; private set; }
    internal SerializedObject serializedObject { get; private set; }

    internal SerializedProperty baseProperty;


    Editor m_Inspector;

    internal ComponentSettingsBaseEditor()
    {
    }

    /// <summary>
    /// Repaints the inspector.
    /// </summary>
    public void Repaint()
    {
        m_Inspector.Repaint();
    }

    internal void Init(WorldObjectDataComponentSettings target, Editor inspector)
    {
        this.target = target;
        m_Inspector = inspector;
        serializedObject = new SerializedObject(target);

        OnEnable();
    }

    /// <summary>
    /// Called when the editor is initialized.
    /// </summary>
    public virtual void OnEnable()
    {
    }

    /// <summary>
    /// Called when the editor is de-initialized.
    /// </summary>
    public virtual void OnDisable()
    {
    }

    internal void OnInternalInspectorGUI()
    {
        serializedObject.Update();
        OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Called every time the inspector is being redrawn. This is where you should add your UI
    /// drawing code.
    /// </summary>
    public virtual void OnInspectorGUI()
    {
    }

    /// <summary>
    /// Returns the label to use as the effect title. You can override this to return a custom
    /// label, else it will use the effect type as the title.
    /// </summary>
    /// <returns>The label to use as the effect title</returns>
    public virtual string GetDisplayTitle()
    {
        return ObjectNames.NicifyVariableName(target.GetType().Name);
    }

}
