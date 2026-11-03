using UnityEngine;

[CreateAssetMenu(menuName = "BoredBro/Detection Cone Style", fileName = "NewConeStyle")]
public class ConeStyle : ScriptableObject
{
    public Material material;                          // Any material, including custom shaders
    public bool useTint = true;                        // Tick off if the material already has the color you want
    public Color tint = new Color(1f, 1f, 0f, 0.25f);  // Applied per-renderer, so the material asset is never modified
    public string colorProperty = "_BaseColor";        // URP Unlit = _BaseColor, Built-in/Sprites = _Color, custom shader = your property name
}