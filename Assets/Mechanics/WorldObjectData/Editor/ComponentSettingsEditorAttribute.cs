using System;

/// <summary>
/// Tells a <see cref="ComponentSettingsEffectEditor{T}"/> class which run-time type it's an editor
/// for. When you make a custom editor for an effect, you need put this attribute on the editor
/// class.
/// </summary>
/// <seealso cref="ComponentSettingsEffectEditor{T}"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ComponentSettingsEditorAttribute : Attribute
{
    /// <summary>
    /// The type that this editor can edit.
    /// </summary>
    public readonly Type settingsType;

    /// <summary>
    /// Creates a new attribute.
    /// </summary>
    /// <param name="settingsType">The type that this editor can edit</param>
    public ComponentSettingsEditorAttribute(Type settingsType)
    {
        this.settingsType = settingsType;
    }
}
