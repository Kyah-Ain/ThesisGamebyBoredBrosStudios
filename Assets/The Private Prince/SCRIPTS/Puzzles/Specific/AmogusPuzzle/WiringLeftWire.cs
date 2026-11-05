using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WiringLeftWire : MonoBehaviour
{
    [Header("Fixed UI")]
    [SerializeField] private Image wireDesign;
    [SerializeField] private TextMeshProUGUI symbolText;

    [Header("Movable Wire")]
    [SerializeField] private RectTransform plugOrigin;
    [SerializeField] private RectTransform plug;
    [SerializeField] private Image plugImage;
    [SerializeField] private RectTransform wireLine;
    [SerializeField] private Image wireLineImage;

    [Header("Selection")]
    [SerializeField] private GameObject selectionIndicator;

    public int PairID { get; private set; }

    public RectTransform Plug => plug;
    public RectTransform PlugOrigin => plugOrigin;
    public RectTransform WireLine => wireLine;

    public bool Connected { get; private set; }

    private RectTransform puzzleRoot;
    private RectTransform dragLayer;

    private Color wireColor;

    public void Initialize(
        int pairID,
        Color color,
        string symbol,
        RectTransform puzzleRoot,
        RectTransform dragLayer)
    {
        PairID = pairID;

        this.puzzleRoot = puzzleRoot;
        this.dragLayer = dragLayer;

        wireColor = color;

        Connected = false;

        if (wireDesign != null)
            wireDesign.color = color;

        if (plugImage != null)
            plugImage.color = color;

        if (wireLineImage != null)
            wireLineImage.color = color;

        if (symbolText != null)
            symbolText.text = symbol;

        SetSelected(false);

        ResetPlug();
    }

    public void BeginMoving()
    {
        /*
         * Move these objects to a shared puzzle-space layer.
         *
         * This prevents their movement from being restricted by
         * the LeftWirePrefab's local RectTransform.
         */

        if (wireLine != null)
            wireLine.SetParent(dragLayer, true);

        if (plug != null)
            plug.SetParent(dragLayer, true);

        /*
         * Static plug rotation.
         *
         * The line rotates, but the plug itself doesn't.
         */
        plug.rotation = Quaternion.identity;

        UpdateLine();
    }

    public void MovePlug(Vector2 movement)
    {
        if (Connected || plug == null)
            return;

        Vector2 newPosition =
            plug.anchoredPosition + movement;

        plug.anchoredPosition = newPosition;

        /*
         * Intentionally do NOT rotate the plug.
         */
        plug.localRotation = Quaternion.identity;

        UpdateLine();
    }

    public void SetPlugPosition(Vector2 rootPosition)
    {
        if (plug == null)
            return;

        plug.anchoredPosition = rootPosition;

        plug.localRotation = Quaternion.identity;

        UpdateLine();
    }

    public void SnapTo(RectTransform target)
    {
        if (target == null)
            return;

        Vector2 targetPosition =
            WorldToRootPosition(target.position);

        plug.anchoredPosition = targetPosition;

        plug.localRotation = Quaternion.identity;

        Connected = true;

        UpdateLine();

        SetSelected(false);
    }

    public void ResetPlug()
    {
        Connected = false;

        if (plug == null || plugOrigin == null)
            return;

        Vector3 worldPosition = plugOrigin.position;

        if (dragLayer != null)
            plug.SetParent(dragLayer, true);

        plug.position = worldPosition;
        plug.localRotation = Quaternion.identity;

        if (wireLine != null && dragLayer != null)
            wireLine.SetParent(dragLayer, true);

        UpdateLine();
    }

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }

    public void UpdateLine()
    {
        if (wireLine == null ||
            plugOrigin == null ||
            plug == null ||
            puzzleRoot == null)
        {
            return;
        }

        Vector2 start =
            WorldToRootPosition(plugOrigin.position);

        Vector2 end =
            WorldToRootPosition(plug.position);

        Vector2 direction = end - start;

        float distance = direction.magnitude;

        /*
         * Line begins at its left edge.
         */
        wireLine.pivot = new Vector2(0f, 0.5f);

        wireLine.anchoredPosition = start;

        /*
         * Only LENGTH changes.
         *
         * Height/thickness remains unchanged.
         */
        wireLine.sizeDelta =
            new Vector2(
                distance,
                wireLine.sizeDelta.y
            );

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        wireLine.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    public Vector2 WorldToRootPosition(Vector3 worldPosition)
    {
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                worldPosition
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            puzzleRoot,
            screenPosition,
            null,
            out Vector2 localPosition
        );

        return localPosition;
    }
}