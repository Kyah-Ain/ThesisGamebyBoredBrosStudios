using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuCell : MonoBehaviour
{
    [Header("UI")]
    public Button button;

    public TMP_Text numberText;

    public TMP_Text pencilText;

    [Header("Optional Visuals")]
    public GameObject selectedVisual;

    public GameObject fixedVisual;

    private SudokuPuzzle puzzle;

    private int row;
    private int column;

    private bool isFixed;

    private readonly HashSet<int> pencilMarks =
        new HashSet<int>();

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void Initialize(
        SudokuPuzzle puzzle,
        int row,
        int column)
    {
        this.puzzle = puzzle;

        this.row = row;
        this.column = column;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                OnClicked
            );
        }
    }

    private void OnClicked()
    {
        if (puzzle == null)
            return;

        puzzle.SelectCell(
            row,
            column
        );
    }

    public void SetNumber(int number)
    {
        if (number <= 0)
        {
            numberText.text = "";

            return;
        }

        numberText.text =
            number.ToString();

        /*
         * A final number replaces pencil marks.
         */

        ClearPencilMarks();
    }

    public void SetFixed(bool fixedCell)
    {
        isFixed = fixedCell;

        if (fixedVisual != null)
        {
            fixedVisual.SetActive(
                fixedCell
            );
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedVisual != null)
        {
            selectedVisual.SetActive(
                selected
            );
        }
    }

    public void TogglePencilMark(int number)
    {
        if (pencilMarks.Contains(number))
        {
            pencilMarks.Remove(number);
        }
        else
        {
            pencilMarks.Add(number);
        }

        UpdatePencilDisplay();
    }

    public void ClearPencilMarks()
    {
        pencilMarks.Clear();

        UpdatePencilDisplay();
    }

    private void UpdatePencilDisplay()
    {
        if (pencilText == null)
            return;

        if (pencilMarks.Count == 0)
        {
            pencilText.text = "";

            return;
        }

        List<int> sortedMarks =
            new List<int>(pencilMarks);

        sortedMarks.Sort();

        StringBuilder builder =
            new StringBuilder();

        foreach (int mark in sortedMarks)
        {
            builder.Append(mark);
            builder.Append(" ");
        }

        pencilText.text =
            builder.ToString();
    }

    public void ShowError()
    {
        /*
         * This method intentionally doesn't permanently
         * change the cell's appearance.
         *
         * You can replace this with an Animator,
         * Image color change, shake animation, etc.
         */

        if (numberText != null)
        {
            numberText.text = "X";
        }

        CancelInvoke(nameof(ClearError));

        Invoke(
            nameof(ClearError),
            0.25f
        );
    }

    private void ClearError()
    {
        numberText.text = "";
    }
}