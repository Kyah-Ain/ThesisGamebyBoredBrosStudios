using UnityEngine;
using UnityEngine.UI;

public class WirePath : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image wireFillImage;

    public bool HasWire { get; private set; }
    public Color WireColor { get; private set; }
    public bool IsCurrentWire { get; private set; }

    public void Initialize(float cellSize)
    {
        // Scale background to fill cell
        if (backgroundImage != null)
        {
            backgroundImage.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        }
        if (wireFillImage != null)
        {
            wireFillImage.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        }
    }

    public void SetTemporaryWire(Color color)
    {
        HasWire = true;
        WireColor = color;
        IsCurrentWire = true;
    }

    public void SetPermanentWire(Color color)
    {
        HasWire = true;
        WireColor = color;
        IsCurrentWire = false;
    }

    public void ClearWire()
    {
        HasWire = false;
        IsCurrentWire = false;
    }
}