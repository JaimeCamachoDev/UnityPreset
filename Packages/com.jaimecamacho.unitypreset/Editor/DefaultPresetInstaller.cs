using UnityEditor;
using UnityEditor.Presets;

[InitializeOnLoad]
internal static class DefaultPresetInstaller
{
    static DefaultPresetInstaller()
    {
        RegisterTexturePresets();
        RegisterModelPresets();
    }

    static void RegisterTexturePresets()
    {
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Albedo.preset", "");
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Normal.preset", "name:*_N*");
        AddPreset<TextureImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Textures/TI_Lightmap.preset", "path:*/Lightmaps/*");
    }

    static void RegisterModelPresets()
    {
        AddPreset<ModelImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/MI_FBX_Static.preset", "label:static");
        AddPreset<ModelImporter>("Packages/com.jaimecamacho.unitypreset/Presets/Importers/Models/MI_FBX_Animated.preset", "label:animated");
    }

    static void AddPreset<T>(string presetPath, string filter) where T : AssetImporter
    {
        var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
        if (preset == null)
            return;

        var type = typeof(T);
        foreach (var existing in PresetManager.GetDefaultPresetsForType(type))
        {
            if (existing.preset == preset && existing.filter == filter)
                return;
        }
        PresetManager.AddDefaultPreset(type, filter, preset);
    }
}
