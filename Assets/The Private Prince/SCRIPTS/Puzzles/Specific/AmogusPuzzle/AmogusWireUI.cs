using UnityEngine;
using UnityEngine.UI;

public class AmogusWireUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform body;
    [SerializeField] private RectTransform startCap;
    [SerializeField] private RectTransform endCap;

    [SerializeField] private Image bodyImage;

    [Header("Appearance")]
    [SerializeField]
    private float wireThickness = 12f;

    [SerializeField]
    private float capSize = 28f;

    private RectTransform rectTransform;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        body.gameObject.SetActive(false);
        startCap.gameObject.SetActive(false);
        endCap.gameObject.SetActive(false);
    }

    /// <summary>
    /// Begins drawing the wire.
    /// </summary>
    public void Begin(Vector2 start, Color wireColor)
    {
        startPosition = start;
        endPosition = start;

        body.gameObject.SetActive(true);
        startCap.gameObject.SetActive(true);
        endCap.gameObject.SetActive(true);

        bodyImage.color = wireColor;

        UpdateEnd(start);
    }

    /// <summary>
    /// Updates the free end while dragging.
    /// </summary>
    public void UpdateEnd(Vector2 end)
    {
        endPosition = end;

        RefreshVisual();
    }

    /// <summary>
    /// Snaps to the final position.
    /// </summary>
    public void Finish(Vector2 end)
    {
        endPosition = end;

        RefreshVisual();
    }

    /// <summary>
    /// Cancels the wire.
    /// </summary>
    public void Cancel()
    {
        Destroy(gameObject);
    }

    private void RefreshVisual()
    {
        Vector2 direction = endPosition - startPosition;

        float length = direction.magnitude;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        //-----------------------------------
        // Root
        //-----------------------------------

        rectTransform.anchoredPosition =
            (startPosition + endPosition) * 0.5f;

        rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, angle);

        //-----------------------------------
        // Body
        //-----------------------------------

        body.sizeDelta =
            new Vector2(
            Mathf.Max(0f, length - capSize),
            wireThickness);
        //-----------------------------------
        // Start Cap
        //-----------------------------------

        startCap.sizeDelta =
            Vector2.one * capSize;

        startCap.anchoredPosition =
            new Vector2(-length * 0.5f, 0f);

        //-----------------------------------
        // End Cap
        //-----------------------------------

        endCap.sizeDelta =
            Vector2.one * capSize;

        endCap.anchoredPosition =
            new Vector2(length * 0.5f, 0f);
    }
}