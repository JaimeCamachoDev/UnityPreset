# Unity Preset Toolkit

Paquete con Presets y una herramienta de instalación para acelerar la importación de assets.

## Presets incluidos

### Texturas
- **VZ_Textures**: ajustes generales para texturas.
- **VZ_Normal**: configuración para mapas de normales.

### Modelos FBX
- **VZ_FBX_Static**: desactiva la importación de animaciones.
- **VZ_FBX_Animated**: activa la importación de animaciones.

## Instalación de los presets
El paquete no modifica el Preset Manager automáticamente al importarlo (así no pisa nada en proyectos que ya tengan sus propios presets configurados). Para registrar los presets del paquete como defaults del proyecto, ve a:

**Tools → JaimeCamachoDev → Presets → Install Presets**

Esto añade las siguientes entradas al Preset Manager, conservando cualquier otra entrada que el proyecto ya tuviera para otros tipos:

| Tipo | Filtro | Preset |
| --- | --- | --- |
| `TextureImporter` | `glob:"2-Art/1-3D/**/*"` | VZ_Textures |
| `TextureImporter` | `glob:"*_Normal.*"` | VZ_Normal |
| `ModelImporter` | `glob:"*Geo.*"` | VZ_FBX_Static |
| `ModelImporter` | `glob:"*Anim.*"` | VZ_FBX_Animated |

Puedes modificar los filtros o el orden desde **Project Settings > Preset Manager** una vez instalados. El botón es idempotente: puedes volver a pulsarlo (por ejemplo tras actualizar el paquete) sin generar entradas duplicadas.

## Uso
1. Importa el paquete.
2. Ejecuta **Tools → JaimeCamachoDev → Presets → Install Presets**.
3. Etiqueta tus modelos con `Geo`/`Anim` en el nombre y usa sufijos como `_Normal` para mapas de normales, según los filtros de la tabla anterior.
