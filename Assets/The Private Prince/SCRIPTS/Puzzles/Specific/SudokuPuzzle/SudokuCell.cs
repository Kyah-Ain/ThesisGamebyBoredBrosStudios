using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuCell : MonoBehaviour
{
    [Header("Visual Root")]
    [Tooltip("Assign the empty GameObject that contains all visual UI components.")]
    public RectTransform visualRoot;

    [Tooltip("The size the cell prefab was originally designed at.")]
    public float referenceCellSize = 200f;

    [Header("UI")]
    public Image normalImage;
    public Image prefilledImage;
    public Image selectionImage;
    public TextMeshProUGUI numberText;

    [Header("Text Colors")]
    public Color editableTextColor = Color.white;
    public Color prefilledTextColor = Color.white;
    public Color wrongTextColor = Color.red;

    [HideInInspector] public int row;
    [HideInInspector] public int column;

    private int value;
    private bool prefilled;
    private bool wrong;

    public int Value => value;
    public bool IsPrefilled => prefilled;

    public void Setup(
        int row,
        int column,
        float actualCellSize)
    {
        this.row = row;
        this.column = column;

        value = 0;
        prefilled = false;
        wrong = false;

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

        /*
         * GridLayoutGroup changes the size of the SudokuCell root,
         * but does not automatically scale its children.
         *
         * visualRoot remains at the prefab's reference size and we
         * scale the entire visual hierarchy together.
         */

        visualRoot.anchorMin = new Vector2(0.5f, 0.5f);
        visualRoot.anchorMax = new Vector2(0.5f, 0.5f);
        visualRoot.pivot = new Vector2(0.5f, 0.5f);

        visualRoot.anchoredPosition = Vector2.zero;

        visualRoot.sizeDelta =
            new Vector2(referenceCellSize, referenceCellSize);

        float scale =
            actualCellSize / referenceCellSize;

        visualRoot.localScale =
            Vector3.one * scale;
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

    // ---------------------------------------------------------
    // SELECTION
    // ---------------------------------------------------------

    public void SetSelected(bool selected)
    {
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
        if (numberText != null)
        {
            numberText.text =
                value > 0 ? value.ToString() : "";

            if (wrong)
            {
                numberText.color = wrongTextColor;
            }
            else if (prefilled)
            {
                numberText.color = prefilledTextColor;
            }
            else
            {
                numberText.color = editableTextColor;
            }
        }

        if (prefilledImage != null)
        {
            prefilledImage.gameObject.SetActive(prefilled);
        }

        if (normalImage != null)
        {
            normalImage.gameObject.SetActive(!prefilled);
        }
    }
}