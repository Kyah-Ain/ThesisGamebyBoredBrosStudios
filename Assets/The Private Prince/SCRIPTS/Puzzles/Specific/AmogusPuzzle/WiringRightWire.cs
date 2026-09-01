using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WiringRightWire : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image wireDesign;
    [SerializeField] private TextMeshProUGUI symbolText;
    [SerializeField] private RectTransform snapPoint;

    public int PairID { get; private set; }

    public RectTransform SnapPoint => snapPoint;

    public void Initialize(int pairID, Color color, string symbol)
    {
        PairID = pairID;

        if (wireDesign != null)
            wireDesign.color = color;

        if (symbolText != null)
            symbolText.text = symbol;
    }
}