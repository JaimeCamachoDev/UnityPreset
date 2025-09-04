using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

[InitializeOnLoad]
internal static class DefaultPresetInstaller
{
    static DefaultPresetInstaller()
    {
        RegisterTexturePresets();
        RegisterModelPresets();
        Debug.Log("[UnityPreset] Default presets installed on load");
    }

    [MenuItem("Tools/JaimeCamachoDev/Presets/Install Default Presets")]
    [MenuItem("Assets/JaimeCamachoDev/Presets/Install Default Presets")]
    static void InstallFromMenu()
    {
        RegisterTexturePresets();
        RegisterModelPresets();
        Debug.Log("[UnityPreset] Default presets installed from menu");
    }
  
    static void RegisterTexturePresets()
    {
        AddPreset<TextureImporter>(
            "Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/VZ_Textures.preset",
            "glob:\"2-Art/1-3D/**/*\"");
        AddPreset<TextureImporter>(
            "Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/VZ_Normal.preset",
            "glob:\"*_Normal.*\"");
    }

    static void RegisterModelPresets()
    {
        AddPreset<ModelImporter>(
            "Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/VZ_FBX_Static.preset",
            "glob:\"2-Art/1-3D/**/*\"");
        AddPreset<ModelImporter>(
            "Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/VZ_FBX_Animated.preset",
            "glob:\"2-Art/1-3D/**/*\"");
    }

    static void AddPreset<T>(string presetPath, string filter) where T : AssetImporter
    {
        var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
        if (preset == null)
        {
            Debug.LogWarning($"[UnityPreset] Preset not found at {presetPath}");
            return;
        }

        var type = typeof(T);
        var presetManagerType = typeof(Preset).Assembly.GetType("UnityEditor.Presets.PresetManager");
        if (presetManagerType == null)
        {
            Debug.LogWarning("[UnityPreset] PresetManager type not found");
            return;
        }

        var getDefaults = presetManagerType.GetMethod("GetDefaultPresetsForType", BindingFlags.Static | BindingFlags.Public);
        var addDefault = presetManagerType.GetMethod("AddDefaultPreset", BindingFlags.Static | BindingFlags.Public);
        var removeDefault = presetManagerType.GetMethod("RemoveDefaultPreset", BindingFlags.Static | BindingFlags.Public);
        if (getDefaults == null || addDefault == null || removeDefault == null)
        {
            Debug.LogWarning("[UnityPreset] PresetManager methods not found");
            return;
        }

        var defaults = (IEnumerable)getDefaults.Invoke(null, new object[] { type });
        foreach (var entry in defaults)
        {
            var entryType = entry.GetType();
            var presetField = entryType.GetField("preset");
            var filterField = entryType.GetField("filter");
            if (presetField == null || filterField == null)
                continue;

            var existingPreset = presetField.GetValue(entry) as Preset;
            var existingFilter = filterField.GetValue(entry) as string;
            if (existingFilter == filter)
            {
                if (existingPreset == preset)
                {
                    Debug.Log($"[UnityPreset] Preset {preset.name} already registered for {type.Name} with filter '{filter}'");
                    return;
                }

                removeDefault.Invoke(null, new object[] { type, existingFilter, existingPreset });
                Debug.Log($"[UnityPreset] Removed preset {existingPreset.name} for {type.Name} with filter '{existingFilter}'");
            }
        }

        addDefault.Invoke(null, new object[] { type, filter, preset });
        Debug.Log($"[UnityPreset] Registered preset {preset.name} for {type.Name} with filter '{filter}'");
    }
}
