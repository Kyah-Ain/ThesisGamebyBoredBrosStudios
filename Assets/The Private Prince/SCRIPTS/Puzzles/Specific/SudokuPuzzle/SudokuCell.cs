using UnityEngine;
using UnityEngine.UI;

public class SudokuCell : MonoBehaviour
{
    [Header("Visual Root")]
    [Tooltip("Assign the empty GameObject that contains all visual UI components.")]
    public RectTransform visualRoot;

    [Tooltip("The size the cell prefab was originally designed at.")]
    public float referenceCellSize = 100f;

    [Header("UI")]
    public Image normalImage;
    public Image prefilledImage;
    public Image selectionImage;
    public Image numberImage;

    [Header("Number Colors")]
    public Color editableNumberColor = Color.white;
    public Color prefilledNumberColor = Color.white;
    public Color wrongNumberColor = Color.red;
    public Color previewNumberColor = Color.gray;

    [HideInInspector] public int row;
    [HideInInspector] public int column;

    private Sprite[] numberSprites;

    private int value;
    private bool prefilled;
    private bool wrong;

    private int previewValue;
    private bool previewing;

    public int Value => value;
    public bool IsPrefilled => prefilled;

    public void Setup(
        int row,
        int column,
        float actualCellSize,
        Sprite[] numberSprites)
    {
        this.row = row;
        this.column = column;
        this.numberSprites = numberSprites;

        value = 0;
        prefilled = false;
        wrong = false;
        previewing = false;
        previewValue = 0;

        ScaleVisuals(actualCellSize);
        RefreshVisuals();

        SetSelected(false);
    }

    // ---------------------------------------------------------
    // SCALING
    // ---------------------------------------------------------

    private void ScaleVisuals(float actualCellSize)
    {
        if (visualRoot == null)
            return;

        visualRoot.anchorMin = new Vector2(0.5f, 0.5f);
        visualRoot.anchorMax = new Vector2(0.5f, 0.5f);
        visualRoot.pivot = new Vector2(0.5f, 0.5f);

        visualRoot.anchoredPosition = Vector2.zero;

        visualRoot.sizeDelta =
            new Vector2(referenceCellSize, referenceCellSize);

        float scale = actualCellSize / referenceCellSize;

        visualRoot.localScale = Vector3.one * scale;
    }

    // ---------------------------------------------------------
    // VALUE
    // ---------------------------------------------------------

    public void SetValue(int newValue)
    {
        value = newValue;
        RefreshVisuals();
    }

    public void SetPrefilled(bool isPrefilled)
    {
        prefilled = isPrefilled;
        RefreshVisuals();
    }

    public void SetWrong(bool isWrong)
    {
        wrong = isWrong;
        RefreshVisuals();
    }

    public void SetPreview(int number)
    {
        previewing = true;
        previewValue = number;
        RefreshVisuals();
    }

    public void ClearPreview()
    {
        if (!previewing)
            return;

        previewing = false;
        previewValue = 0;
        RefreshVisuals();
    }

    // ---------------------------------------------------------
    // SELECTION
    // ---------------------------------------------------------

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            // Cells are positioned manually, so sibling order only
            // affects draw order. This keeps the selection image from
            // being covered by cells created after this one.
            transform.SetAsLastSibling();
        }

        if (selectionImage != null)
        {
            selectionImage.gameObject.SetActive(selected);
        }
    }

    // ---------------------------------------------------------
    // VISUALS
    // ---------------------------------------------------------

    private void RefreshVisuals()
    {
        if (numberImage != null)
        {
            int displayValue = previewing ? previewValue : value;

            bool hasSprite =
                displayValue > 0 &&
                numberSprites != null &&
                displayValue <= numberSprites.Length &&
                numberSprites[displayValue - 1] != null;

            numberImage.enabled = hasSprite;

            if (hasSprite)
            {
                numberImage.sprite = numberSprites[displayValue - 1];
                numberImage.SetNativeSize();
            }

            if (previewing)
                numberImage.color = previewNumberColor;
            else if (wrong)
                numberImage.color = wrongNumberColor;
            else if (prefilled)
                numberImage.color = prefilledNumberColor;
            else
                numberImage.color = editableNumberColor;
        }

        if (prefilledImage != null)
            prefilledImage.gameObject.SetActive(prefilled);

        if (normalImage != null)
            normalImage.gameObject.SetActive(!prefilled);
    }
}