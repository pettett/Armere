using System;


/// <summary>
/// Use this attribute to associate a <see cref="PostProcessEffectSettings"/> to a
/// <see cref="PostProcessEffectRenderer{T}"/> type.
/// </summary>
/// <seealso cref="PostProcessEffectSettings"/>
/// <seealso cref="PostProcessEffectRenderer{T}"/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class WorldObjectComponentAttribute : Attribute
{
    /// <summary>
    /// The renderer type to associate with a <see cref="PostProcessEffectSettings"/>.
    /// </summary>
    public readonly Type monoBehaviour;


    /// <summary>
    /// The menu item name to set for the effect. You can use a `/` character to add sub-menus.
    /// </summary>
    public readonly string menuItem;


    /// <summary>
    /// Creates a new attribute.
    /// </summary>
    /// <param name="renderer">The renderer type to associate with a <see cref="PostProcessEffectSettings"/></param>
    /// <param name="eventType">The injection point for the effect</param>
    /// <param name="menuItem">The menu item name to set for the effect. You can use a `/` character to add sub-menus.</param>
    /// <param name="allowInSceneView">Should this effect be allowed in the Scene View?</param>
    public WorldObjectComponentAttribute(Type monoBehaviour, string menuItem)
    {
        this.monoBehaviour = monoBehaviour;
        this.menuItem = menuItem;

    }
}
