using UnityEngine;
using UnityEngine.UI;

public class WiringRightWire : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image wireDesign;
    [SerializeField] private Image symbolImage;
    [SerializeField] private RectTransform snapPoint;

    public int PairID { get; private set; }

    public RectTransform SnapPoint => snapPoint;

    /*
     * The left wire currently plugged into this one (right or wrong).
     */
    public WiringLeftWire Occupant { get; private set; }

    public bool IsOccupied => Occupant != null;

    public void Initialize(int pairID, Color color, Sprite symbol)
    {
        PairID = pairID;
        Occupant = null;

        if (wireDesign != null)
            wireDesign.color = color;

        if (symbolImage != null)
        {
            symbolImage.sprite = symbol;
            symbolImage.enabled = symbol != null;
        }
    }

    public void SetOccupant(WiringLeftWire wire)
    {
        Occupant = wire;
    }

    public void ClearOccupant()
    {
        Occupant = null;
    }
}