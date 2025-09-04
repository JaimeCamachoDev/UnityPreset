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
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Albedo.preset", "");
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Normal.preset", "name:*_N*");
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Lightmap.preset", "path:*/Lightmaps/*");
    }

    static void RegisterModelPresets()
    {
        AddPreset<ModelImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/MI_FBX_Static.preset", "");
        AddPreset<ModelImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/MI_FBX_Animated.preset", "label:animated");
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
        if (getDefaults == null || addDefault == null)
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
            if (existingPreset == preset && existingFilter == filter)
            {
                Debug.Log($"[UnityPreset] Preset {preset.name} already registered for {type.Name} with filter '{filter}'");
                return;
            }
        }

        addDefault.Invoke(null, new object[] { type, filter, preset });
        Debug.Log($"[UnityPreset] Registered preset {preset.name} for {type.Name} with filter '{filter}'");
    }
}
