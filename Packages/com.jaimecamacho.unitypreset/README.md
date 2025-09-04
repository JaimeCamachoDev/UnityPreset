# Unity Preset Toolkit

Paquete con Presets y script de instalación automática para acelerar la importación de assets.

## Presets incluidos

### Texturas
- **TI_Albedo**: textura estándar con sRGB y mipmaps.
- **TI_Normal**: mapa de normales con sRGB desactivado.
- **TI_Lightmap**: lightmap en espacio lineal sin mipmaps.

### Modelos FBX
- **MI_FBX_Static**: desactiva la importación de animaciones.
- **MI_FBX_Animated**: activa la importación de animaciones.

## Instalación automática
Al importar el paquete, el script `DefaultPresetInstaller` añade las siguientes entradas al Preset Manager:

| Tipo | Filtro | Preset |
| --- | --- | --- |
| `TextureImporter` | *(sin filtro)* | TI_Albedo |
| `TextureImporter` | `name:*_N*` | TI_Normal |
| `TextureImporter` | `path:*/Lightmaps/*` | TI_Lightmap |
| `ModelImporter` | `label:static` | MI_FBX_Static |
| `ModelImporter` | `label:animated` | MI_FBX_Animated |

Puedes modificar los filtros desde **Project Settings > Preset Manager** según tu organización de assets.

## Uso
1. Importa el paquete.
2. Los presets se registran automáticamente. Revisa el Preset Manager si deseas cambiar filtros o prioridades.
3. Etiqueta tus modelos con `static` o `animated` y usa sufijos como `_N` para mapas de normales para que se apliquen los presets correctos.
