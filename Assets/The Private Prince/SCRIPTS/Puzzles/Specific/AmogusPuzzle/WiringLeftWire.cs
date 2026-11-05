using UnityEngine;
using UnityEngine.UI;

public class WiringLeftWire : MonoBehaviour
{
    [Header("Fixed UI")]
    [SerializeField] private Image wireDesign;
    [SerializeField] private Image symbolImage;

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

    /*
     * True only when plugged into the CORRECT right wire.
     * Solved wires are locked and can't be selected again.
     */
    public bool Solved { get; private set; }

    /*
     * The right wire this plug currently sits on (correct or not).
     * Null when the plug is free or at its origin.
     */
    public WiringRightWire PluggedInto { get; private set; }

    private RectTransform puzzleRoot;
    private RectTransform dragLayer;

    public void Initialize(
        int pairID,
        Color color,
        Sprite symbol,
        RectTransform puzzleRoot,
        RectTransform dragLayer)
    {
        PairID = pairID;

        this.puzzleRoot = puzzleRoot;
        this.dragLayer = dragLayer;

        Solved = false;
        PluggedInto = null;

        if (wireDesign != null)
            wireDesign.color = color;

        if (wireLineImage != null)
            wireLineImage.color = color;

        if (symbolImage != null)
        {
            symbolImage.sprite = symbol;
            symbolImage.enabled = symbol != null;
        }

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
        if (Solved || plug == null)
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

    /*
     * Plug into a right wire. If it isn't the matching one the
     * wire stays unsolved (and can be picked up again), but the
     * right wire is marked occupied so nothing else snaps to it.
     */
    public void SnapTo(WiringRightWire target, bool correct)
    {
        if (target == null)
            return;

        Vector2 targetPosition =
            WorldToRootPosition(target.SnapPoint.position);

        plug.anchoredPosition = targetPosition;

        plug.localRotation = Quaternion.identity;

        PluggedInto = target;
        target.SetOccupant(this);

        Solved = correct;

        UpdateLine();

        SetSelected(false);
    }

    /*
     * Detach from whatever right wire we're sitting on.
     * Does not move the plug.
     */
    public void Unplug()
    {
        if (PluggedInto != null)
            PluggedInto.ClearOccupant();

        PluggedInto = null;
        Solved = false;
    }

    /*
     * Full reset: unplug and send the plug back to its origin.
     */
    public void ResetWire()
    {
        Unplug();
        ResetPlug();
        SetSelected(false);
    }

    /*
     * Moves the plug (and line) back to the plug origin.
     */
    public void ResetPlug()
    {
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