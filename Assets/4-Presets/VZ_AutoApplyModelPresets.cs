// Assets/4-Presets/Editor/VZ_ApplyModelPresets.cs
// Aplica automáticamente los presets "VZ_FBX_Static" y "VZ_FBX_Animated"
// a los modelos FBX usando heurística para decidir si es estático o animado.
// Compatible con Unity 2020+ (no usa PresetType ni PresetManager).

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public static class VZ_ApplyModelPresets
{
    private const string STATIC_PRESET_NAME = "VZ_FBX_Static";
    private const string ANIM_PRESET_NAME = "VZ_FBX_Animated";

    // ===== MENÚS =====
    [MenuItem("Tools/VZ Presets/Apply to ALL Models (Auto-detect)")]
    private static void ApplyToAllModels()
    {
        var modelGuids = FindAllModelGuidsInProject();
        ApplyInternal(modelGuids);
    }

    [MenuItem("Tools/VZ Presets/Apply to SELECTED (Folders/Models) (Auto-detect)")]
    private static void ApplyToSelection()
    {
        var modelGuids = FindModelGuidsFromSelection();
        ApplyInternal(modelGuids);
    }

    // ===== NÚCLEO =====
    private static void ApplyInternal(List<string> modelGuids)
    {
        var presetStatic = FindPresetByName(STATIC_PRESET_NAME);
        var presetAnimated = FindPresetByName(ANIM_PRESET_NAME);

        if (presetStatic == null || presetAnimated == null)
        {
            EditorUtility.DisplayDialog(
                "VZ Presets",
                "No se han encontrado los presets requeridos:\n" +
                $"- {STATIC_PRESET_NAME}\n- {ANIM_PRESET_NAME}\n\n" +
                "Verifica que existen y que se llaman exactamente así.",
                "OK"
            );
            return;
        }

        int scanned = 0;
        int changed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < modelGuids.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                EditorUtility.DisplayProgressBar("VZ Presets", $"Analizando {path}", (float)i / Mathf.Max(1, modelGuids.Count));

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue; // seguridad

                scanned++;

                // Heurística para decidir Animated vs Static
                bool looksAnimated = importer.importAnimation ||
                                     importer.animationType != ModelImporterAnimationType.None;

                var presetToApply = looksAnimated ? presetAnimated : presetStatic;

                if (!presetToApply.CanBeAppliedTo(importer))
                    continue;

                presetToApply.ApplyTo(importer);
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        EditorUtility.DisplayDialog(
            "VZ Presets",
            $"Proceso completado.\n\n" +
            $"Escaneados: {scanned}\n" +
            $"Modificados: {changed}\n\n" +
            $"Static preset:   {AssetDatabase.GetAssetPath(presetStatic)}\n" +
            $"Animated preset: {AssetDatabase.GetAssetPath(presetAnimated)}",
            "OK"
        );
    }

    // ===== UTILIDADES =====
    private static Preset FindPresetByName(string presetName)
    {
        // Busca por nombre exacto en todo el proyecto
        var guids = AssetDatabase.FindAssets($"{presetName} t:Preset", new[] { "Assets" });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var p = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (p != null && p.name == presetName)
                return p;
        }
        return null;
    }

    private static List<string> FindAllModelGuidsInProject()
    {
        // "t:Model" = assets importados por ModelImporter (FBX, etc.) dentro de Assets (no Packages)
        var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        return new List<string>(guids);
    }

    private static List<string> FindModelGuidsFromSelection()
    {
        var result = new List<string>();
        var selection = Selection.objects;
        if (selection == null || selection.Length == 0) return result;

        var folders = new List<string>();
        foreach (var obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            if (AssetDatabase.IsValidFolder(path))
            {
                folders.Add(path);
            }
            else
            {
                // Si es un modelo suelto
                var mi = AssetImporter.GetAtPath(path) as ModelImporter;
                if (mi != null)
                {
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid)) result.Add(guid);
                }
            }
        }

        if (folders.Count > 0)
        {
            var folderGuids = AssetDatabase.FindAssets("t:Model", folders.ToArray());
            result.AddRange(folderGuids);
        }

        return result;
    }
}
#endif
