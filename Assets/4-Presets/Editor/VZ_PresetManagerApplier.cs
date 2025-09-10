using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public static class VZ_PresetManagerApplier
{
    [MenuItem("Tools/VZ Presets/Apply VZ presets")]
    private static void ApplyPresetManagerPresets()
    {
        // Fetch default presets for ModelImporter from Preset Manager using reflection
        var defaults = GetDefaultPresetsForModelImporter();
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

    // ---------------------------------------------------------------
    //  Reflection helpers to support multiple Unity versions
    // ---------------------------------------------------------------
    private static Preset[] GetDefaultPresetsForModelImporter()
    {
        var presetTypeType = typeof(Preset).Assembly.GetType("UnityEditor.Presets.PresetType");
        if (presetTypeType == null)
            return null;

        var presetTypeInstance = CreatePresetTypeForModelImporter(presetTypeType);
        if (presetTypeInstance != null)
        {
            var method = typeof(Preset).GetMethod("GetDefaultPresetsForType",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { presetTypeType },
                null);

            if (method != null)
                return method.Invoke(null, new[] { presetTypeInstance }) as Preset[];
        }

        var methodType = typeof(Preset).GetMethod("GetDefaultPresetsForType",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Type) },
            null);

        if (methodType != null)
            return methodType.Invoke(null, new object[] { typeof(ModelImporter) }) as Preset[];

        return null;
    }

    private static object CreatePresetTypeForModelImporter(Type presetTypeType)
    {
        var ctorType = presetTypeType.GetConstructor(new[] { typeof(Type) });
        if (ctorType != null)
            return ctorType.Invoke(new object[] { typeof(ModelImporter) });

        var ctorObj = presetTypeType.GetConstructor(new[] { typeof(UnityEngine.Object) });
        if (ctorObj != null)
        {
            var anyModelGuid = AssetDatabase.FindAssets("t:Model", new[] { "Assets" }).FirstOrDefault();
            if (!string.IsNullOrEmpty(anyModelGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(anyModelGuid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                    return ctorObj.Invoke(new object[] { importer });
            }
        }

        var ctorString = presetTypeType.GetConstructor(new[] { typeof(string) });
        if (ctorString != null)
            return ctorString.Invoke(new object[] { "ModelImporter" });

        return null;
    }
}
