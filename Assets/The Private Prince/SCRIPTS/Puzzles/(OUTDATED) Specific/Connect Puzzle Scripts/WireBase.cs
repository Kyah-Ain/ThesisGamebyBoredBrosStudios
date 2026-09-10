using UnityEngine;
using UnityEngine.UI;

public class WireBase : MonoBehaviour
{
    [SerializeField] private Image dotImage;
    [SerializeField] private Image glowImage;

    public Color BaseColor { get; private set; }
    public bool IsGlitch { get; private set; }

    public void Initialize(Color color, bool isGlitch, float cellSize = 80f)
    {
        BaseColor = color;
        IsGlitch = isGlitch;

        dotImage.color = color;
        glowImage.color = new Color(color.r, color.g, color.b, 0.3f);

        float dotScale = Mathf.Clamp(cellSize / 100f, 0.5f, 1.5f); // Adjust scale factor as needed
        dotImage.transform.localScale = Vector3.one * dotScale * 0.8f; // 80% of cell
        glowImage.transform.localScale = Vector3.one * dotScale;

        // Glitches might have different visual appearance
        if (isGlitch)
        {
            // Add glitch effect - could be animation, different shape, etc.
            dotImage.transform.localScale = Vector3.one * 0.8f;
        }
    }
}