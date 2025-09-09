// Assets/4-Presets/Editor/VZ_ModelImportRules.cs
#if UNITY_EDITOR
using UnityEngine;

public class VZ_ModelImportRules : ScriptableObject
{
    [Header("Carpeta de FBX estáticos (dentro de Assets/)")]
    public string staticFolder = "Assets/2-Art/1-3D/Static";

    [Header("Carpeta de FBX animados (dentro de Assets/)")]
    public string animatedFolder = "Assets/2-Art/1-3D/Animated";

    [Header("Nombres exactos de los .preset (bajo Assets/)")]
    public string staticPresetName = "VZ_FBX_Static";
    public string animatedPresetName = "VZ_FBX_Animated";
}
#endif
