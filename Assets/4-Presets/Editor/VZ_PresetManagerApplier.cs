using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public static class VZ_PresetManagerApplier
{
    [MenuItem("Tools/VZ Presets/Apply VZ presets")]
    private static void ApplyPresetManagerPresets()
    {
        // Fetch default presets for ModelImporter from Preset Manager
        var defaults = Preset.GetDefaultPresetsForType(new PresetType("ModelImporter"));
        if (defaults == null || defaults.Length == 0)
        {
            EditorUtility.DisplayDialog("VZ Presets", "No default presets found for ModelImporter.", "OK");
            return;
        }

        int scanned = 0;
        int changed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var def in defaults)
            {
                if (def.preset == null) continue;
                string folder = ExtractPathFromFilter(def.filter);
                if (string.IsNullOrEmpty(folder)) continue;

                var modelGuids = AssetDatabase.FindAssets("t:Model", new[] { folder });
                foreach (var guid in modelGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null) continue;

                    scanned++;
                    if (!def.preset.CanBeAppliedTo(importer))
                        continue;

                    def.preset.ApplyTo(importer);
                    importer.SaveAndReimport();
                    changed++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        EditorUtility.DisplayDialog(
            "VZ Presets",
            $"Process finished.\nScanned: {scanned}\nModified: {changed}",
            "OK");
    }

    private static string ExtractPathFromFilter(string filter)
    {
        const string key = "path:\"";
        if (string.IsNullOrEmpty(filter) || !filter.Contains(key))
            return null;

        int start = filter.IndexOf(key) + key.Length;
        int end = filter.IndexOf('\"', start);
        if (end < 0) return null;
        return filter.Substring(start, end - start);
    }
}
