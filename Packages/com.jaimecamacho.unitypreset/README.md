# Unity Preset Toolkit

Paquete con Presets y script de instalación automática para acelerar la importación de assets.

## Presets incluidos

### Texturas
- **VZ_Textures**: ajustes generales para texturas.
- **VZ_Normal**: configuración para mapas de normales.

### Modelos FBX
- **VZ_FBX_Static**: desactiva la importación de animaciones.
- **VZ_FBX_Animated**: activa la importación de animaciones.

## Instalación automática
Al importar el paquete, el script `DefaultPresetInstaller` añade las siguientes entradas al Preset Manager:

| Tipo | Filtro | Preset |
| --- | --- | --- |
| `TextureImporter` | `glob:"2-Art/1-3D/**/*"` | VZ_Textures |
| `TextureImporter` | `glob:"*_Normal.*"` | VZ_Normal |
| `ModelImporter` | `glob:"2-Art/1-3D/**/*"` | VZ_FBX_Static |
| `ModelImporter` | `glob:"2-Art/1-3D/**/*"` | VZ_FBX_Animated |

Puedes modificar los filtros desde **Project Settings > Preset Manager** según tu organización de assets.

## Uso
1. Importa el paquete.
2. Los presets se registran automáticamente. Revisa el Preset Manager si deseas cambiar filtros o prioridades.
3. Etiqueta tus modelos con `static` o `animated` y usa sufijos como `_N` para mapas de normales para que se apliquen los presets correctos.
