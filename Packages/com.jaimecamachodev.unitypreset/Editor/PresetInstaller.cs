using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace JaimeCamachoDev.UnityPreset.Editor
{
    /// <summary>
    /// Registra los presets del paquete como presets por defecto (Project Settings > Preset Manager)
    /// del proyecto en el que esté instalado, sin tocar los defaults ya configurados para otros tipos.
    /// </summary>
    internal static class PresetInstaller
    {
        private static readonly (string relativePath, string filter)[] k_Defaults =
        {
            ("Presets/Importers/Models/VZ_FBX_Animated.preset", "glob:\"*Anim.*\""),
            ("Presets/Importers/Models/VZ_FBX_Static.preset", "glob:\"*Geo.*\""),
            ("Presets/Importers/Textures/VZ_Textures.preset", "glob:\"2-Art/1-3D/**/*\""),
            ("Presets/Importers/Textures/VZ_Normal.preset", "glob:\"*_Normal.*\""),
        };

        [MenuItem("Tools/JaimeCamachoDev/Presets/Install Presets")]
        private static void InstallPresets()
        {
            string packageRoot = GetPackageRootPath();
            int installed = 0;

            foreach (var (relativePath, filter) in k_Defaults)
            {
                string presetPath = $"{packageRoot}/{relativePath}";
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                if (preset == null)
                {
                    Debug.LogWarning($"[UnityPreset] No se encontró el preset en '{presetPath}'.");
                    continue;
                }

                var type = preset.GetPresetType();
                if (!type.IsValidDefault())
                {
                    Debug.LogWarning($"[UnityPreset] El preset '{preset.name}' no admite ser un preset por defecto.");
                    continue;
                }

                // Evita duplicados si el preset ya estaba instalado (p. ej. al reinstalar).
                Preset.RemoveFromDefault(preset);

                var defaults = Preset.GetDefaultPresetsForType(type).ToList();
                defaults.Add(new DefaultPreset(filter, preset));
                Preset.SetDefaultPresetsForType(type, defaults.ToArray());
                installed++;
            }

            EditorUtility.DisplayDialog(
                "UnityPreset",
                installed > 0
                    ? $"Se instalaron {installed} presets por defecto en Project Settings > Preset Manager."
                    : "No se pudo instalar ningún preset. Revisa la consola para más detalles.",
                "OK");
        }

        private static string GetPackageRootPath()
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(PresetInstaller).Assembly);
            return packageInfo != null ? packageInfo.assetPath : "Packages/com.jaimecamachodev.unitypreset";
        }
    }
}
