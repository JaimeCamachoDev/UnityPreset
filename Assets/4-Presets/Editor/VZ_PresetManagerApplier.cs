// Assets/4-Presets/Editor/VZ_PresetManagerApplier.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

/// <summary>
/// Utility that ensures a given Preset is registered in the Preset Manager
/// as the default Preset for <see cref="ModelImporter"/> objects.
/// This fixes compilation issues by correctly using <see cref="PresetType"/>
/// and avoiding comparisons of the <see cref="DefaultPreset"/> struct to
/// <c>null</c>.
/// </summary>
public static class VZ_PresetManagerApplier
{
    private const string PRESET_NAME = "VZ_FBX_Static"; // Replace with the desired preset name

    [MenuItem("Tools/VZ Presets/Register Default Model Preset")]
    public static void RegisterDefaultPreset()
    {
        // Locate the preset asset by name
        var preset = FindPresetByName(PRESET_NAME);
        if (preset == null)
        {
            Debug.LogWarning($"Preset '{PRESET_NAME}' not found in project.");
            return;
        }

        // Correct use of PresetType (fixes CS1503)
        var presetType = new PresetType(typeof(ModelImporter));

        // Retrieve all existing default presets for ModelImporter
        var existing = PresetManager.GetDefaultPresetsForType(presetType);
        foreach (var dp in existing)
        {
            // 'DefaultPreset' is a struct, so compare its preset instead of dp == null (fixes CS0019)
            if (dp.preset == preset)
            {
                Debug.Log("Preset already registered as default.");
                return;
            }
        }

        // Register the preset as the default for ModelImporter
        PresetManager.AddDefaultPreset(presetType, preset);
        Debug.Log($"Preset '{PRESET_NAME}' registered as default for ModelImporter.");
    }

    private static Preset FindPresetByName(string presetName)
    {
        var guids = AssetDatabase.FindAssets($"{presetName} t:Preset", new[] { "Assets" });
        foreach (var g in guids)
        {
            var p = AssetDatabase.LoadAssetAtPath<Preset>(AssetDatabase.GUIDToAssetPath(g));
            if (p != null && p.name == presetName)
                return p;
        }
        return null;
    }
}
#endif
