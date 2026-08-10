using System.Collections.Generic;
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
        // Debe reflejar exactamente las entradas de este proyecto en ProjectSettings/PresetManager.asset.
        // Un mismo preset puede repetirse con varios filtros (p. ej. VZ_Textures cubre dos globs distintos).
        private static readonly (string relativePath, string filter)[] k_Defaults =
        {
            // Audio
            ("Presets/Importers/Audio/VZ_Audio_VOX.preset", "glob:\"*_VOX.*\""),
            ("Presets/Importers/Audio/VZ_Audio_SFX.preset", "glob:\"*_SFX.*\""),
            ("Presets/Importers/Audio/VZ_Audio_OST.preset", "glob:\"*_OST.*\""),

            // Texturas
            ("Presets/Importers/Textures/VZ_Textures.preset", "glob:\"2-Art/1-3D/**/*\""),
            ("Presets/Importers/Textures/VZ_Textures.preset", "glob:\"*_ColorAlpha.*\""),
            ("Presets/Importers/Textures/VZ_Normal.preset", "glob:\"*_Normal.*\""),
            ("Presets/Importers/Textures/VZ_MaskMap.preset", "glob:\"*_MaskMap.*\""),
            ("Presets/Importers/Textures/VZ_MaskMap.preset", "glob:\"*_HDRP.*\""),

            // Modelos FBX
            ("Presets/Importers/Models/VZ_FBX_Animated.preset", "glob:\"*TPose*\""),
            ("Presets/Importers/Models/VZ_FBX_Animated.preset", "glob:\"*Anim.*\""),
            ("Presets/Importers/Models/VZ_FBX_Static.preset", "glob:\"*Geo.*\""),
        };

        [MenuItem("Tools/JaimeCamachoDev/Presets/Install Presets")]
        private static void InstallPresets()
        {
            string packageRoot = GetPackageRootPath();

            var entries = new List<(Preset preset, string filter)>();
            foreach (var (relativePath, filter) in k_Defaults)
            {
                string presetPath = $"{packageRoot}/{relativePath}";
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                if (preset == null)
                {
                    Debug.LogWarning($"[UnityPreset] No se encontró el preset en '{presetPath}'.");
                    continue;
                }

                entries.Add((preset, filter));
            }

            // Primero se limpian todas las entradas previas de estos presets (idempotencia al reinstalar).
            // Se hace en un paso aparte porque un mismo preset puede tener varios filtros: si se limpiara
            // preset a preset justo antes de añadirlo, la segunda entrada de ese preset borraría la primera.
            foreach (var preset in entries.Select(e => e.preset).Distinct())
                Preset.RemoveFromDefault(preset);

            int installed = 0;
            foreach (var group in entries.GroupBy(e => e.preset.GetPresetType()))
            {
                var type = group.Key;
                if (!type.IsValidDefault())
                {
                    Debug.LogWarning($"[UnityPreset] El tipo '{type.GetManagedTypeName()}' no admite presets por defecto.");
                    continue;
                }

                var defaults = Preset.GetDefaultPresetsForType(type).ToList();
                foreach (var (preset, filter) in group)
                {
                    defaults.Add(new DefaultPreset(filter, preset));
                    installed++;
                }
                Preset.SetDefaultPresetsForType(type, defaults.ToArray());
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
