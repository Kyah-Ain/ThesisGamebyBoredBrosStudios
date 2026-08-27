using UnityEngine;

[System.Serializable]
public class AmogusWirePair
{
    [Tooltip("Unique ID used to determine matching wires.")]
    public int pairID;

    [Tooltip("Optional display color.")]
    public Color wireColor = Color.white;

    [Tooltip("Optional wire sprite/icon.")]
    public Sprite wireSprite;
}