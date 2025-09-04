using System;
using System.Collections.Generic;
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

        var importerType = typeof(T);
        var presetType = new PresetType(importerType);
        var defaults = new List<DefaultPreset>(Preset.GetDefaultPresetsForType(presetType));

        var index = defaults.FindIndex(d => d.filter == filter);
        if (index >= 0)
        {
            var existing = defaults[index];
            if (existing.preset == preset)
            {
                Debug.Log($"[UnityPreset] Preset {preset.name} already registered for {importerType.Name} with filter '{filter}'");
                return;
            }

            defaults.RemoveAt(index);
            Debug.Log($"[UnityPreset] Removed preset {existing.preset.name} for {importerType.Name} with filter '{existing.filter}'");
        }

        defaults.Add(new DefaultPreset(filter, preset));
        Preset.SetDefaultPresetsForType(presetType, defaults.ToArray());
        Debug.Log($"[UnityPreset] Registered preset {preset.name} for {importerType.Name} with filter '{filter}'");
    }
}
