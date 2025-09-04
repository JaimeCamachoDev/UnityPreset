# Unity Preset Toolkit

Este paquete contiene Presets reutilizables para proyectos de Unity.

## Presets de Textura

- **TI_UI_Sprite.preset**
  - Tipo de textura: Sprite (2D and UI)
  - sRGB activado
  - Alfa como transparencia activado
  - MipMaps desactivados
  - Compresión: Alta calidad

### Uso

1. Importa este paquete en tu proyecto.
2. Abre **Project Settings > Preset Manager**.
3. Añade una entrada para `TextureImporter` con el filtro `path:Assets/Art/UI/**` y asigna el preset `TI_UI_Sprite`.
4. Las texturas que coincidan con el filtro usarán automáticamente los valores del preset.
