<img width="629" height="341" alt="UnityPreset" src="https://github.com/user-attachments/assets/e3a901db-12f2-4c94-a371-15b7757a7203" />

# **UnityPreset**

_**Presets de importación para Unity (texturas, modelos y audio) con instalador en el Preset Manager**_

## Requisitos

* Unity **6000.0** o superior (el proyecto de desarrollo está en **6000.4.10f1**)

## Instalación

Desde npm, añadiendo en `Packages/manifest.json`:

```json
"scopedRegistries": [
  { "name": "npm", "url": "https://registry.npmjs.org", "scopes": ["com.jaimecamachodev"] }
],
"dependencies": {
  "com.jaimecamachodev.unitypreset": "1.0.15"
}
```

## Uso

1. Ejecuta **Tools → JaimeCamachoDev → Presets → Install Presets**. Registra los presets del paquete en el Preset Manager sin borrar las entradas que ya tenga el proyecto, y se puede repetir sin duplicar nada.
2. Nombra los assets según los filtros para que se les aplique el preset:

| Tipo | Filtros | Preset |
| --- | --- | --- |
| Audio | `*_VOX.*` · `*_SFX.*` · `*_OST.*` | VZ_Audio_VOX · VZ_Audio_SFX · VZ_Audio_OST |
| Texturas | `2-Art/1-3D/**/*` · `*_ColorAlpha.*` | VZ_Textures |
| Texturas | `*_Normal.*` | VZ_Normal |
| Texturas | `*_MaskMap.*` · `*_HDRP.*` | VZ_MaskMap |
| Modelos | `*TPose*` · `*Anim.*` | VZ_FBX_Animated |
| Modelos | `*Geo.*` | VZ_FBX_Static |

Los filtros y el orden se pueden cambiar después en **Project Settings → Preset Manager**.

## Contenido

* **Paquete:** `Packages/com.jaimecamachodev.unitypreset` (presets en `Presets/Importers`, instalador en `Editor/PresetInstaller.cs`)
* **Escena:** `Assets/3-Scenes/Main.unity` (vacía; el proyecto solo sirve para desarrollar el paquete)
