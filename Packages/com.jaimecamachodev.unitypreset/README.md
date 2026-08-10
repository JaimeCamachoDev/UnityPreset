# Unity Preset Toolkit

Paquete con Presets y una herramienta de instalación para acelerar la importación de assets.

## Presets incluidos

### Audio
- **VZ_Audio_VOX**: ajustes para diálogos/voces.
- **VZ_Audio_SFX**: ajustes para efectos de sonido.
- **VZ_Audio_OST**: ajustes para música.

### Texturas
- **VZ_Textures**: ajustes generales para texturas.
- **VZ_Normal**: configuración para mapas de normales.
- **VZ_MaskMap**: configuración para mask maps (incluye HDRP).

### Modelos FBX
- **VZ_FBX_Static**: desactiva la importación de animaciones.
- **VZ_FBX_Animated**: activa la importación de animaciones.

## Instalación de los presets
El paquete no modifica el Preset Manager automáticamente al importarlo (así no pisa nada en proyectos que ya tengan sus propios presets configurados). Para registrar los presets del paquete como defaults del proyecto, ve a:

**Tools → JaimeCamachoDev → Presets → Install Presets**

Esto añade las siguientes entradas al Preset Manager, conservando cualquier otra entrada que el proyecto ya tuviera para otros tipos:

| Tipo | Filtro | Preset |
| --- | --- | --- |
| `AudioImporter` | `glob:"*_VOX.*"` | VZ_Audio_VOX |
| `AudioImporter` | `glob:"*_SFX.*"` | VZ_Audio_SFX |
| `AudioImporter` | `glob:"*_OST.*"` | VZ_Audio_OST |
| `TextureImporter` | `glob:"2-Art/1-3D/**/*"` | VZ_Textures |
| `TextureImporter` | `glob:"*_ColorAlpha.*"` | VZ_Textures |
| `TextureImporter` | `glob:"*_Normal.*"` | VZ_Normal |
| `TextureImporter` | `glob:"*_MaskMap.*"` | VZ_MaskMap |
| `TextureImporter` | `glob:"*_HDRP.*"` | VZ_MaskMap |
| `ModelImporter` | `glob:"*TPose*"` | VZ_FBX_Animated |
| `ModelImporter` | `glob:"*Anim.*"` | VZ_FBX_Animated |
| `ModelImporter` | `glob:"*Geo.*"` | VZ_FBX_Static |

Puedes modificar los filtros o el orden desde **Project Settings > Preset Manager** una vez instalados. El botón es idempotente: puedes volver a pulsarlo (por ejemplo tras actualizar el paquete) sin generar entradas duplicadas.

## Uso
1. Importa el paquete.
2. Ejecuta **Tools → JaimeCamachoDev → Presets → Install Presets**.
3. Nombra tus assets según los filtros de la tabla anterior (`_VOX`/`_SFX`/`_OST` para audio, `_Normal`/`_MaskMap`/`_HDRP`/`_ColorAlpha` para texturas, `Geo`/`Anim`/`TPose` para modelos) para que se les aplique el preset correspondiente.
