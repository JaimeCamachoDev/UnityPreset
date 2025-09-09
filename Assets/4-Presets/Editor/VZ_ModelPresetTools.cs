// Assets/4-Presets/Editor/VZ_ModelPresetTools.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public static class VZ_ModelPresetTools
{
    private const string ASSET_PATH = "Assets/4-Presets/VZ_ModelImportRules.asset";
    private const string STATIC_DEFAULT = "Assets/2-Art/1-3D/Static";
    private const string ANIMATED_DEFAULT = "Assets/2-Art/1-3D/Animated";
    private const string STATIC_PRESET = "VZ_FBX_Static";
    private const string ANIM_PRESET = "VZ_FBX_Animated";

    [MenuItem("Tools/VZ Presets/Create/Show Model Import Rules")]
    private static void CreateOrPingRules()
    {
        var rules = LoadOrCreateRules();
        Selection.activeObject = rules;
        EditorGUIUtility.PingObject(rules);
        EditorUtility.DisplayDialog("VZ Presets",
            "Config abierta en el Inspector.\n" +
            "Edita carpetas y nombres de presets.\n\n" +
            "Esta config hará de 'reglas automáticas' al importar (como el Preset Manager).",
            "OK");
    }

    [MenuItem("Tools/VZ Presets/Quick Set Folders (pick folders)")]
    private static void QuickSetFolders()
    {
        var rules = LoadOrCreateRules();

        string s = EditorUtility.OpenFolderPanel("Carpeta de FBX estáticos", Application.dataPath, "");
        if (!string.IsNullOrEmpty(s))
            rules.staticFolder = AbsoluteToAssetsRelative(s);

        string a = EditorUtility.OpenFolderPanel("Carpeta de FBX animados", Application.dataPath, "");
        if (!string.IsNullOrEmpty(a))
            rules.animatedFolder = AbsoluteToAssetsRelative(a);

        EditorUtility.SetDirty(rules);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("VZ Presets", "Carpetas guardadas.", "OK");
    }

    [MenuItem("Tools/VZ Presets/Apply to ALL Models (from rules)")]
    private static void ApplyAllFromRules()
    {
        var rules = LoadOrCreateRules();

        var presetStatic = FindPresetByName(rules.staticPresetName);
        var presetAnimated = FindPresetByName(rules.animatedPresetName);

        if (presetStatic == null || presetAnimated == null)
        {
            EditorUtility.DisplayDialog("VZ Presets",
                "Faltan presets (usa nombres exactos en la config):\n" +
                $"- {rules.staticPresetName}\n- {rules.animatedPresetName}", "OK");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        int scanned = 0, changed = 0;

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("VZ Presets", path, (float)i / Mathf.Max(1, guids.Length));
                var mi = AssetImporter.GetAtPath(path) as ModelImporter;
                if (mi == null) continue;
                scanned++;

                Preset p = null;
                var normStatic = Normalize(rules.staticFolder);
                var normAnimated = Normalize(rules.animatedFolder);
                var normPath = path.Replace('\\', '/');

                if (!string.IsNullOrEmpty(normStatic) && normPath.StartsWith(normStatic))
                    p = presetStatic;
                else if (!string.IsNullOrEmpty(normAnimated) && normPath.StartsWith(normAnimated))
                    p = presetAnimated;
                else
                    p = mi.importAnimation ? presetAnimated : presetStatic;

                if (p != null && p.CanBeAppliedTo(mi))
                {
                    p.ApplyTo(mi);
                    mi.SaveAndReimport();
                    changed++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        EditorUtility.DisplayDialog("VZ Presets",
            $"Proceso completado.\nEscaneados: {scanned}\nModificados: {changed}", "OK");
    }

    // ---------- helpers ----------
    private static VZ_ModelImportRules LoadOrCreateRules()
    {
        var rules = AssetDatabase.LoadAssetAtPath<VZ_ModelImportRules>(ASSET_PATH);
        if (rules == null)
        {
            rules = ScriptableObject.CreateInstance<VZ_ModelImportRules>();
            rules.staticFolder = STATIC_DEFAULT;
            rules.animatedFolder = ANIMATED_DEFAULT;
            rules.staticPresetName = STATIC_PRESET;
            rules.animatedPresetName = ANIM_PRESET;
            AssetDatabase.CreateAsset(rules, ASSET_PATH);
            AssetDatabase.SaveAssets();
        }
        return rules;
    }

    private static string Normalize(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return null;
        folder = folder.Replace('\\', '/');
        if (!folder.StartsWith("Assets")) return null;
        if (!folder.EndsWith("/")) folder += "/";
        return folder;
    }

    private static string AbsoluteToAssetsRelative(string abs)
    {
        if (string.IsNullOrEmpty(abs)) return null;
        abs = abs.Replace('\\', '/');
        var data = Application.dataPath.Replace('\\', '/');
        if (!abs.StartsWith(data)) return null;
        var rel = "Assets" + abs.Substring(data.Length);
        if (rel.EndsWith("/")) rel = rel.TrimEnd('/');
        return rel;
    }

    private static Preset FindPresetByName(string presetName)
    {
        var guids = AssetDatabase.FindAssets($"{presetName} t:Preset", new[] { "Assets" });
        foreach (var g in guids)
        {
            var p = AssetDatabase.LoadAssetAtPath<Preset>(AssetDatabase.GUIDToAssetPath(g));
            if (p != null && p.name == presetName) return p;
        }
        return null;
    }
}
#endif
