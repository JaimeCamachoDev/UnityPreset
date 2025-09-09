// Assets/4-Presets/Editor/VZ_ModelImportProcessor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

public class VZ_ModelImportProcessor : AssetPostprocessor
{
    private static VZ_ModelImportRules _rules;
    private static VZ_ModelImportRules Rules
    {
        get
        {
            if (_rules != null) return _rules;
            var guids = AssetDatabase.FindAssets("t:VZ_ModelImportRules", new[] { "Assets" });
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _rules = AssetDatabase.LoadAssetAtPath<VZ_ModelImportRules>(path);
            }
            return _rules;
        }
    }

    void OnPreprocessModel()
    {
        if (Rules == null) return; // sin config, no hacemos nada

        var mi = (ModelImporter)assetImporter;
        string path = mi.assetPath.Replace('\\', '/');

        // Resuelve presets por nombre
        Preset staticPreset = FindPresetByName(Rules.staticPresetName);
        Preset animatedPreset = FindPresetByName(Rules.animatedPresetName);

        // Aplica por carpeta
        if (!string.IsNullOrEmpty(Rules.staticFolder) &&
            path.StartsWith(NormalizeFolder(Rules.staticFolder)) &&
            staticPreset != null && staticPreset.CanBeAppliedTo(mi))
        {
            staticPreset.ApplyTo(mi);
            return;
        }

        if (!string.IsNullOrEmpty(Rules.animatedFolder) &&
            path.StartsWith(NormalizeFolder(Rules.animatedFolder)) &&
            animatedPreset != null && animatedPreset.CanBeAppliedTo(mi))
        {
            animatedPreset.ApplyTo(mi);
            return;
        }

        // Si no coincide carpeta, intenta heurística (opcional)
        if (animatedPreset != null && mi.importAnimation && animatedPreset.CanBeAppliedTo(mi))
        {
            animatedPreset.ApplyTo(mi);
        }
        else if (staticPreset != null && staticPreset.CanBeAppliedTo(mi))
        {
            staticPreset.ApplyTo(mi);
        }
    }

    private static string NormalizeFolder(string folder)
    {
        folder = folder.Replace('\\', '/');
        if (!folder.StartsWith("Assets")) folder = "Assets";
        // Garantizar barra final para StartsWith de rutas
        if (!folder.EndsWith("/")) folder += "/";
        return folder;
    }

    private static Preset FindPresetByName(string presetName)
    {
        if (string.IsNullOrEmpty(presetName)) return null;
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
