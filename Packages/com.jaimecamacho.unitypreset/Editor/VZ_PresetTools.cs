// Assets/4-Presets/Editor/VZ_PresetTools.cs
// Unity 6.2 compatible (y versiones cercanas): aplica presets a FBX y registra reglas
// en Preset Manager para ModelImporter, usando REFLEXIÓN para API variables.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public static class VZ_PresetTools
{
    private const string STATIC_PRESET_NAME = "VZ_FBX_Static";
    private const string ANIM_PRESET_NAME = "VZ_FBX_Animated";

    // =======================
    //  MENÚ: APLICAR PRESETS
    // =======================
    [MenuItem("Tools/VZ Presets/Apply to ALL Models (Auto-detect)")]
    private static void ApplyToAllModels()
    {
        var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        ApplyPresetsToModelGuids(new List<string>(guids));
    }

    [MenuItem("Tools/VZ Presets/Apply to SELECTED (Folders/Models) (Auto-detect)")]
    private static void ApplyToSelection()
    {
        var guids = FindModelGuidsFromSelection();
        ApplyPresetsToModelGuids(guids);
    }

    private static void ApplyPresetsToModelGuids(List<string> modelGuids)
    {
        var presetStatic = FindPresetByName(STATIC_PRESET_NAME);
        var presetAnimated = FindPresetByName(ANIM_PRESET_NAME);

        if (presetStatic == null || presetAnimated == null)
        {
            EditorUtility.DisplayDialog(
                "VZ Presets",
                "No se han encontrado los presets requeridos:\n" +
                $"- {STATIC_PRESET_NAME}\n- {ANIM_PRESET_NAME}\n\n" +
                "Comprueba que existen y que el nombre es EXACTO.",
                "OK");
            return;
        }

        int scanned = 0, changed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < modelGuids.Count; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                EditorUtility.DisplayProgressBar("VZ Presets", $"Analizando {path}", (float)i / Mathf.Max(1, modelGuids.Count));

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                scanned++;

                bool looksAnimated = importer.importAnimation ||
                                     importer.animationType != ModelImporterAnimationType.None;

                var preset = looksAnimated ? presetAnimated : presetStatic;
                if (!preset.CanBeAppliedTo(importer)) continue;

                preset.ApplyTo(importer);
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
            $"Proceso completado.\n\nEscaneados: {scanned}\nModificados: {changed}\n\n" +
            $"Static preset:   {AssetDatabase.GetAssetPath(presetStatic)}\n" +
            $"Animated preset: {AssetDatabase.GetAssetPath(presetAnimated)}",
            "OK");
    }

    private static List<string> FindModelGuidsFromSelection()
    {
        var result = new List<string>();
        var folders = new List<string>();
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            if (AssetDatabase.IsValidFolder(path))
                folders.Add(path);
            else
            {
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
            var guids = AssetDatabase.FindAssets("t:Model", folders.ToArray());
            result.AddRange(guids);
        }
        return result;
    }

    private static Preset FindPresetByName(string presetName)
    {
        var guids = AssetDatabase.FindAssets($"{presetName} t:Preset", new[] { "Assets" });
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var p = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (p != null && p.name == presetName) return p;
        }
        return null;
    }

    // ==========================================
    //  MENÚ: EDITAR PRESET MANAGER (ModelImporter)
    // ==========================================
    [MenuItem("Tools/VZ Presets/Preset Manager ▸ Create/Update Rules (pick folders)")]
    private static void RegisterPresetManagerRules()
    {
        var pStatic = FindPresetByName(STATIC_PRESET_NAME);
        var pAnim = FindPresetByName(ANIM_PRESET_NAME);

        if (pStatic == null || pAnim == null)
        {
            EditorUtility.DisplayDialog(
                "VZ Presets",
                "No se han encontrado los presets requeridos:\n" +
                $"- {STATIC_PRESET_NAME}\n- {ANIM_PRESET_NAME}\n\n" +
                "Crea/renombra los .preset primero.",
                "OK");
            return;
        }

        string staticFolderAbs = EditorUtility.OpenFolderPanel("Carpeta para FBX estáticos", Application.dataPath, "");
        string animatedFolderAbs = EditorUtility.OpenFolderPanel("Carpeta para FBX animados", Application.dataPath, "");

        int rules = 0;

        if (!string.IsNullOrEmpty(staticFolderAbs))
        {
            var rel = AbsoluteToAssetsRelative(staticFolderAbs);
            if (!string.IsNullOrEmpty(rel) && AssetDatabase.IsValidFolder(rel))
            {
                if (EnsureModelImporterRule(pStatic, $"path:\"{rel}\"")) rules++;
            }
            else
            {
                Debug.LogWarning($"[VZ Presets] Ruta inválida para regla Static: {staticFolderAbs}");
            }
        }

        if (!string.IsNullOrEmpty(animatedFolderAbs))
        {
            var rel = AbsoluteToAssetsRelative(animatedFolderAbs);
            if (!string.IsNullOrEmpty(rel) && AssetDatabase.IsValidFolder(rel))
            {
                if (EnsureModelImporterRule(pAnim, $"path:\"{rel}\"")) rules++;
            }
            else
            {
                Debug.LogWarning($"[VZ Presets] Ruta inválida para regla Animated: {animatedFolderAbs}");
            }
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "VZ Presets",
            rules > 0
                ? $"Reglas creadas/actualizadas en Project Settings ▸ Preset Manager.\nReglas: {rules}"
                : "No se creó ninguna regla (cancelaste o rutas inválidas).",
            "OK");
    }

    // ==== Implementación robusta por REFLEXIÓN (soporta cambios de API) ====

    /// <summary>
    /// Crea/actualiza una regla de ModelImporter => preset con el filtro indicado.
    /// Devuelve true si añadió/reemplazó algo, false si ya existía idéntica.
    /// </summary>
    private static bool EnsureModelImporterRule(Preset preset, string filter)
    {
        // Resolvemos tipos del namespace UnityEditor.Presets
        var unityEditorAsm = typeof(Preset).Assembly;
        var presetTypeType = unityEditorAsm.GetType("UnityEditor.Presets.PresetType");
        var presetManagerType = unityEditorAsm.GetType("UnityEditor.Presets.PresetManager"); // puede ser internal

        if (presetTypeType == null)
        {
            Debug.LogError("[VZ Presets] No se encontró UnityEditor.Presets.PresetType. Tu versión no soporta reglas por código.");
            return false;
        }

        // Construimos un PresetType para ModelImporter intentando varios ctors:
        object presetType = CreatePresetTypeForModelImporter(presetTypeType);
        if (presetType == null)
        {
            Debug.LogError("[VZ Presets] No se pudo construir PresetType(ModelImporter) (API distinta).");
            return false;
        }

        // 1) Obtener reglas existentes
        var defaults = GetDefaultPresetsForType(presetTypeType, presetType);
        if (defaults == null)
        {
            Debug.LogWarning("[VZ Presets] No se pudieron leer reglas existentes (seguimos igualmente).");
        }
        else
        {
            // ¿Ya existe la misma regla?
            foreach (var d in defaults)
            {
                var dPreset = GetFieldOrProp<Preset>(d, "preset");
                var dFilter = GetFieldOrProp<string>(d, "filter");
                if (dPreset == preset && dFilter == filter)
                {
                    // Regla idéntica ya existe → nada que hacer
                    return false;
                }
            }

            // Eliminar cualquier regla que use exactamente el mismo filtro (para reemplazar)
            foreach (var d in defaults)
            {
                var dFilter = GetFieldOrProp<string>(d, "filter");
                if (dFilter == filter)
                {
                    TryInvokeRemoveDefaultPreset(presetTypeType, presetManagerType, GetFieldOrProp<Preset>(d, "preset"), presetType, dFilter);
                }
            }
        }

        // 2) Añadir la regla nueva
        bool added = TryInvokeAddDefaultPreset(presetTypeType, presetManagerType, preset, presetType, filter);
        if (added)
            Debug.Log($"[VZ Presets] Regla añadida: {preset.name} → {filter}");
        else
            Debug.LogError("[VZ Presets] No se pudo añadir la regla (métodos no disponibles en esta versión).");

        return added;
    }

    /// Crea PresetType para ModelImporter intentando ctors {Type}, {UnityEngine.Object}, {string}
    private static object CreatePresetTypeForModelImporter(Type presetTypeType)
    {
        // 1) ctor(System.Type)
        var ctorType = presetTypeType.GetConstructor(new[] { typeof(Type) });
        if (ctorType != null) return ctorType.Invoke(new object[] { typeof(ModelImporter) });

        // 2) ctor(UnityEngine.Object)
        var ctorObj = presetTypeType.GetConstructor(new[] { typeof(UnityEngine.Object) });
        if (ctorObj != null)
        {
            // Obtener cualquier ModelImporter real del proyecto
            var anyModelGuid = AssetDatabase.FindAssets("t:Model", new[] { "Assets" }).FirstOrDefault();
            if (!string.IsNullOrEmpty(anyModelGuid))
            {
                var path = AssetDatabase.GUIDToAssetPath(anyModelGuid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null) return ctorObj.Invoke(new object[] { importer });
            }
        }

        // 3) ctor(string) con el nombre del tipo
        var ctorString = presetTypeType.GetConstructor(new[] { typeof(string) });
        if (ctorString != null) return ctorString.Invoke(new object[] { "ModelImporter" });

        return null;
    }

    /// Obtiene array de DefaultPreset por reflexión, devolviendo objetos sin tipar.
    private static Array GetDefaultPresetsForType(Type presetTypeType, object presetTypeInstance)
    {
        // Primero probamos Preset.GetDefaultPresetsForType(PresetType)
        var method = typeof(Preset).GetMethod("GetDefaultPresetsForType",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { presetTypeType },
            null);

        if (method != null)
            return method.Invoke(null, new[] { presetTypeInstance }) as Array;

        // Alternativa: Preset.GetDefaultPresetsForType(Type)
        method = typeof(Preset).GetMethod("GetDefaultPresetsForType",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Type) },
            null);

        if (method != null)
            return method.Invoke(null, new object[] { typeof(ModelImporter) }) as Array;

        // Alternativa: PresetManager.GetDefaultPresetsForType(...), aunque sea internal
        var presetManagerType = typeof(Preset).Assembly.GetType("UnityEditor.Presets.PresetManager");
        if (presetManagerType != null)
        {
            // probamos con PresetType
            method = presetManagerType.GetMethod("GetDefaultPresetsForType",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { presetTypeType },
                null);
            if (method != null)
                return method.Invoke(null, new[] { presetTypeInstance }) as Array;

            // probamos con Type
            method = presetManagerType.GetMethod("GetDefaultPresetsForType",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(Type) },
                null);
            if (method != null)
                return method.Invoke(null, new object[] { typeof(ModelImporter) }) as Array;
        }

        return null;
    }

    private static bool TryInvokeAddDefaultPreset(Type presetTypeType, Type presetManagerType, Preset preset, object presetTypeInstance, string filter)
    {
        // 1) Preset.AddDefaultPreset(Preset, PresetType, string)
        var m = typeof(Preset).GetMethod("AddDefaultPreset",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Preset), presetTypeType, typeof(string) },
            null);
        if (m != null) { m.Invoke(null, new object[] { preset, presetTypeInstance, filter }); return true; }

        // 2) PresetManager.AddDefaultPreset(...), aunque sea internal
        if (presetManagerType != null)
        {
            // buscamos cualquier AddDefaultPreset con 3 args que encajen (Preset, *, string)
            foreach (var mm in presetManagerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (mm.Name != "AddDefaultPreset") continue;
                var p = mm.GetParameters();
                if (p.Length != 3) continue;

                object[] args = new object[3];
                bool ok = true;
                for (int i = 0; i < 3; i++)
                {
                    var t = p[i].ParameterType;
                    if (typeof(Preset).IsAssignableFrom(t)) args[i] = preset;
                    else if (t == typeof(string)) args[i] = filter;
                    else if (t == presetTypeType || t == typeof(Type)) args[i] = t == typeof(Type) ? typeof(ModelImporter) : presetTypeInstance;
                    else { ok = false; break; }
                }
                if (ok) { mm.Invoke(null, args); return true; }
            }
        }
        return false;
    }

    private static void TryInvokeRemoveDefaultPreset(Type presetTypeType, Type presetManagerType, Preset preset, object presetTypeInstance, string filter)
    {
        // 1) Preset.RemoveDefaultPreset(Preset, PresetType, string)
        var m = typeof(Preset).GetMethod("RemoveDefaultPreset",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Preset), presetTypeType, typeof(string) },
            null);
        if (m != null) { m.Invoke(null, new object[] { preset, presetTypeInstance, filter }); return; }

        // 2) PresetManager.RemoveDefaultPreset(...)
        if (presetManagerType != null)
        {
            foreach (var mm in presetManagerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (mm.Name != "RemoveDefaultPreset") continue;
                var p = mm.GetParameters();
                if (p.Length != 3) continue;

                object[] args = new object[3];
                bool ok = true;
                for (int i = 0; i < 3; i++)
                {
                    var t = p[i].ParameterType;
                    if (typeof(Preset).IsAssignableFrom(t)) args[i] = preset;
                    else if (t == typeof(string)) args[i] = filter;
                    else if (t == presetTypeType || t == typeof(Type)) args[i] = t == typeof(Type) ? typeof(ModelImporter) : presetTypeInstance;
                    else { ok = false; break; }
                }
                if (ok) { mm.Invoke(null, args); return; }
            }
        }
    }

    private static T GetFieldOrProp<T>(object obj, string name)
    {
        if (obj == null) return default;
        var t = obj.GetType();
        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null) return (T)f.GetValue(obj);
        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null) return (T)p.GetValue(obj);
        return default;
    }

    private static string AbsoluteToAssetsRelative(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return null;
        absolutePath = absolutePath.Replace('\\', '/');
        string dataPath = Application.dataPath.Replace('\\', '/');
        if (!absolutePath.StartsWith(dataPath)) return null;
        return "Assets" + absolutePath.Substring(dataPath.Length);
    }
}
#endif
