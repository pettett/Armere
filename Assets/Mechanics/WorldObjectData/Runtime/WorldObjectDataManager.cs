using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

using UnityEngine;

/// <summary>
/// This manager tracks all volumes in the scene and does all the interpolation work. It is
/// automatically created as soon as Post-processing is active in a scene.
/// </summary>
public sealed class WorldObjectDataManager
{
    static WorldObjectDataManager s_Instance;

    /// <summary>
    /// The current singleton instance of <see cref="WorldObjectDataManager"/>.
    /// </summary>
    public static WorldObjectDataManager instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = new WorldObjectDataManager();

            return s_Instance;
        }
    }

    /// <summary>
    /// This dictionary maps all <see cref="PostProcessEffectSettings"/> available to their
    /// corresponding <see cref="WorldObjectComponentAttribute"/>. It can be used to list all loaded
    /// builtin and custom effects.
    /// </summary>
    public readonly Dictionary<Type, WorldObjectComponentAttribute> settingsTypes;

    WorldObjectDataManager()
    {


        settingsTypes = new Dictionary<Type, WorldObjectComponentAttribute>();
        ReloadBaseTypes();
    }

#if UNITY_EDITOR
    // Called every time Unity recompile scripts in the editor. We need this to keep track of
    // any new custom effect the user might add to the project
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnEditorReload()
    {
        instance.ReloadBaseTypes();
    }
#endif

    void CleanBaseTypes()
    {
        settingsTypes.Clear();

    }

    // This will be called only once at runtime and everytime script reload kicks-in in the
    // editor as we need to keep track of any compatible post-processing effects in the project
    void ReloadBaseTypes()
    {
        CleanBaseTypes();

        // Rebuild the base type map
        var types = RuntimeUtilities.GetAllTypesDerivedFrom<WorldObjectDataComponentSettings>()
                        .Where(
                            t => t.IsDefined(typeof(WorldObjectComponentAttribute), false)
                              && !t.IsAbstract
                        );

        foreach (var type in types)
        {
            //Record all the base types that can be then used to turn the data into a created object
            settingsTypes.Add(type, type.GetAttribute<WorldObjectComponentAttribute>());
        }
    }





}
