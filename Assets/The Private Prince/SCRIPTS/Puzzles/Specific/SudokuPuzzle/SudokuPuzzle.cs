using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuPuzzle : PuzzleBase
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Serializable]
    private class DifficultySettings
    {
        public int size;
        public int boxRows;
        public int boxColumns;
    }

    [Header("Sudoku Settings")]
    public Difficulty difficulty = Difficulty.Easy;

    /*
     * Developer slider.
     *
     * This represents the number of cells that are already filled
     * when the puzzle starts.
     *
     * Easy  = 4x4  = 16 cells
     * Medium = 6x6 = 36 cells
     * Hard   = 9x9 = 81 cells
     */
    [Range(1, 80)]
    public int filledCells = 8;

    [Tooltip("If true, an incorrect number is immediately rejected.")]
    public bool rejectWrongAnswers = true;

    [Tooltip("If true, pencil marks are cleared when a number is entered.")]
    public bool clearPencilsWhenNumberEntered = true;

    [Header("Grid References")]
    public Transform gridParent;
    public SudokuCell cellPrefab;
    public SudokuGridLayout gridLayout;

    [Header("Number Input")]
    public Button[] numberButtons;
    public Button clearButton;

    [Header("Pencil Mode")]
    public Toggle pencilModeToggle;

    [Header("UI")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI mistakeText;

    private int[,] solution;
    private int[,] playerGrid;

    private bool[,] fixedCells;

    private SudokuCell[,] cells;

    private int gridSize;
    private int boxRows;
    private int boxColumns;

    private int selectedRow = -1;
    private int selectedColumn = -1;

    private int mistakes = 0;

    private System.Random random = new System.Random();

    private readonly Dictionary<Difficulty, DifficultySettings> settings =
        new Dictionary<Difficulty, DifficultySettings>()
        {
            {
                Difficulty.Easy,
                new DifficultySettings
                {
                    size = 4,
                    boxRows = 2,
                    boxColumns = 2
                }
            },

            {
                Difficulty.Medium,
                new DifficultySettings
                {
                    size = 6,
                    boxRows = 2,
                    boxColumns = 3
                }
            },

            {
                Difficulty.Hard,
                new DifficultySettings
                {
                    size = 9,
                    boxRows = 3,
                    boxColumns = 3
                }
            }
        };

    private void Awake()
    {
        ApplyDifficultySettings();

        SetupNumberButtons();
        SetupClearButton();

        if (pencilModeToggle != null)
        {
            pencilModeToggle.isOn = false;
            pencilModeToggle.onValueChanged.AddListener(OnPencilModeChanged);
        }
    }

    private void ApplyDifficultySettings()
    {
        DifficultySettings current = settings[difficulty];

        gridSize = current.size;
        boxRows = current.boxRows;
        boxColumns = current.boxColumns;

        /*
         * Prevent the developer from filling every cell.
         * There needs to be at least one cell for the player to solve.
         */
        filledCells = Mathf.Clamp(
            filledCells,
            1,
            gridSize * gridSize - 1
        );
    }

    public override void StartPuzzle()
    {
        ApplyDifficultySettings();

        GeneratePuzzle();

        base.StartPuzzle();

        SelectCell(0, 0);

        UpdateMistakeUI();
        SetStatus("Solve the Sudoku!");
    }

    public override void ResetPuzzle()
    {
        base.ResetPuzzle();

        GeneratePuzzle();

        SelectCell(0, 0);

        mistakes = 0;

        UpdateMistakeUI();
        SetStatus("Puzzle reset.");
    }

    public override void HandleInput()
    {
        /*
         * Keyboard input.
         *
         * This allows the player to use the keyboard instead of
         * clicking the number buttons.
         */

        for (int i = 1; i <= gridSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                EnterNumber(i);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                EnterNumber(i);
                return;
            }
        }

        /*
         * Delete / Backspace clears the selected cell.
         */
        if (Input.GetKeyDown(KeyCode.Delete) ||
            Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearSelectedCell();
        }
    }

    // =========================================================
    // PUZZLE GENERATION
    // =========================================================

    private void GeneratePuzzle()
    {
        ApplyDifficultySettings();

        solution = new int[gridSize, gridSize];
        playerGrid = new int[gridSize, gridSize];
        fixedCells = new bool[gridSize, gridSize];

        GenerateSolvedGrid();

        CreatePuzzleFromSolution();

        BuildUI();
    }

    private void GenerateSolvedGrid()
    {
        /*
         * Creates a valid base Sudoku grid.
         *
         * The formula works for:
         *
         * 4x4  -> 2x2 boxes
         * 6x6  -> 2x3 boxes
         * 9x9  -> 3x3 boxes
         */

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                solution[row, column] =
                    (row * boxColumns +
                     row / boxRows +
                     column) % gridSize + 1;
            }
        }

        /*
         * Randomize the solved board while keeping it valid.
         */
        ShuffleDigits();
        ShuffleRows();
        ShuffleColumns();
    }

    private void ShuffleDigits()
    {
        List<int> digits = new List<int>();

        for (int i = 1; i <= gridSize; i++)
        {
            digits.Add(i);
        }

        ShuffleList(digits);

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                solution[row, column] =
                    digits[solution[row, column] - 1];
            }
        }
    }

    private void ShuffleRows()
    {
        /*
         * Shuffle rows inside their respective regions.
         */

        for (int group = 0; group < gridSize / boxRows; group++)
        {
            List<int> rows = new List<int>();

            for (int i = 0; i < boxRows; i++)
            {
                rows.Add(group * boxRows + i);
            }

            ShuffleList(rows);

            int[,] temp = CopyArray(solution);

            for (int i = 0; i < boxRows; i++)
            {
                int destinationRow = group * boxRows + i;
                int sourceRow = rows[i];

                for (int column = 0; column < gridSize; column++)
                {
                    solution[destinationRow, column] =
                        temp[sourceRow, column];
                }
            }
        }

        /*
         * Shuffle entire row groups.
         */
        List<int> groups = new List<int>();

        for (int i = 0; i < gridSize / boxRows; i++)
        {
            groups.Add(i);
        }

        ShuffleList(groups);

        int[,] groupedCopy = CopyArray(solution);

        for (int group = 0; group < groups.Count; group++)
        {
            int sourceGroup = groups[group];

            for (int row = 0; row < boxRows; row++)
            {
                int destinationRow =
                    group * boxRows + row;

                int sourceRow =
                    sourceGroup * boxRows + row;

                for (int column = 0; column < gridSize; column++)
                {
                    solution[destinationRow, column] =
                        groupedCopy[sourceRow, column];
                }
            }
        }
    }

    private void ShuffleColumns()
    {
        /*
         * Same concept as row shuffling,
         * but applied to columns.
         */

        for (int group = 0; group < gridSize / boxColumns; group++)
        {
            List<int> columns = new List<int>();

            for (int i = 0; i < boxColumns; i++)
            {
                columns.Add(group * boxColumns + i);
            }

            ShuffleList(columns);

            int[,] temp = CopyArray(solution);

            for (int i = 0; i < boxColumns; i++)
            {
                int destinationColumn =
                    group * boxColumns + i;

                int sourceColumn =
                    columns[i];

                for (int row = 0; row < gridSize; row++)
                {
                    solution[row, destinationColumn] =
                        temp[row, sourceColumn];
                }
            }
        }

        /*
         * Shuffle entire column groups.
         */

        List<int> groups = new List<int>();

        for (int i = 0; i < gridSize / boxColumns; i++)
        {
            groups.Add(i);
        }

        ShuffleList(groups);

        int[,] groupedCopy = CopyArray(solution);

        for (int group = 0; group < groups.Count; group++)
        {
            int sourceGroup = groups[group];

            for (int column = 0; column < boxColumns; column++)
            {
                int destinationColumn =
                    group * boxColumns + column;

                int sourceColumn =
                    sourceGroup * boxColumns + column;

                for (int row = 0; row < gridSize; row++)
                {
                    solution[row, destinationColumn] =
                        groupedCopy[row, sourceColumn];
                }
            }
        }
    }

    private void CreatePuzzleFromSolution()
    {
        /*
         * Start with an empty player board.
         */

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                playerGrid[row, column] = 0;
                fixedCells[row, column] = false;
            }
        }

        /*
         * Randomly choose which cells are initially filled.
         */

        List<int> positions = new List<int>();

        for (int i = 0; i < gridSize * gridSize; i++)
        {
            positions.Add(i);
        }

        ShuffleList(positions);

        for (int i = 0; i < filledCells; i++)
        {
            int position = positions[i];

            int row = position / gridSize;
            int column = position % gridSize;

            playerGrid[row, column] =
                solution[row, column];

            fixedCells[row, column] = true;
        }
    }

    // =========================================================
    // UI
    // =========================================================

    private void BuildUI()
    {
        if (gridParent == null || cellPrefab == null)
        {
            Debug.LogError(
                "SudokuPuzzle: Grid Parent or Cell Prefab is missing."
            );

            return;
        }

        // Remove previous cells.
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Destroy(gridParent.GetChild(i).gameObject);
        }

        cells = new SudokuCell[gridSize, gridSize];

        // Generate cells.
        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                SudokuCell cell =
                    Instantiate(cellPrefab, gridParent);

                cells[row, column] = cell;

                int capturedRow = row;
                int capturedColumn = column;

                cell.Initialize(
                    this,
                    capturedRow,
                    capturedColumn
                );

                cell.SetNumber(
                    playerGrid[row, column]
                );

                cell.SetFixed(
                    fixedCells[row, column]
                );
            }
        }

        // Configure the grid AFTER creating the cells.
        if (gridLayout != null)
        {
            gridLayout.SetGridSize(gridSize);
        }

        SetupNumberButtonVisibility();
    }

    private void SetupNumberButtons()
    {
        if (numberButtons == null)
            return;

        for (int i = 0; i < numberButtons.Length; i++)
        {
            int value = i + 1;

            if (numberButtons[i] == null)
                continue;

            numberButtons[i].onClick.RemoveAllListeners();

            numberButtons[i].onClick.AddListener(
                () => EnterNumber(value)
            );

            TMP_Text text =
                numberButtons[i].GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text = value.ToString();
            }
        }
    }

    private void SetupNumberButtonVisibility()
    {
        if (numberButtons == null)
            return;

        for (int i = 0; i < numberButtons.Length; i++)
        {
            if (numberButtons[i] == null)
                continue;

            numberButtons[i].gameObject.SetActive(
                i < gridSize
            );
        }
    }

    private void SetupClearButton()
    {
        if (clearButton == null)
            return;

        clearButton.onClick.RemoveAllListeners();

        clearButton.onClick.AddListener(
            ClearSelectedCell
        );
    }

    // =========================================================
    // CELL SELECTION
    // =========================================================

    public void SelectCell(int row, int column)
    {
        if (row < 0 ||
            row >= gridSize ||
            column < 0 ||
            column >= gridSize)
        {
            return;
        }

        selectedRow = row;
        selectedColumn = column;

        UpdateCellSelection();
    }

    private void UpdateCellSelection()
    {
        if (cells == null)
            return;

        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                bool selected =
                    row == selectedRow &&
                    column == selectedColumn;

                cells[row, column].SetSelected(
                    selected
                );
            }
        }
    }

    // =========================================================
    // NUMBER INPUT
    // =========================================================

    public void EnterNumber(int number)
    {
        if (!IsActive())
            return;

        if (selectedRow < 0 ||
            selectedColumn < 0)
        {
            return;
        }

        if (number < 1 || number > gridSize)
            return;

        /*
         * The player cannot modify a starting cell.
         */

        if (fixedCells[selectedRow, selectedColumn])
        {
            SetStatus("That cell is already filled.");
            return;
        }

        bool pencilMode =
            pencilModeToggle != null &&
            pencilModeToggle.isOn;

        if (pencilMode)
        {
            TogglePencilMark(number);
            return;
        }

        /*
         * Check whether the answer is correct.
         */

        if (number != solution[selectedRow, selectedColumn])
        {
            mistakes++;

            UpdateMistakeUI();

            cells[selectedRow, selectedColumn]
                .ShowError();

            SetStatus(
                "Wrong number!"
            );

            if (rejectWrongAnswers)
            {
                cells[selectedRow, selectedColumn]
                    .SetNumber(0);

                playerGrid[selectedRow, selectedColumn] = 0;
            }
            else
            {
                playerGrid[selectedRow, selectedColumn] =
                    number;

                cells[selectedRow, selectedColumn]
                    .SetNumber(number);
            }

            return;
        }

        /*
         * Correct answer.
         */

        playerGrid[selectedRow, selectedColumn] =
            number;

        cells[selectedRow, selectedColumn]
            .SetNumber(number);

        if (clearPencilsWhenNumberEntered)
        {
            cells[selectedRow, selectedColumn]
                .ClearPencilMarks();
        }

        SetStatus("Correct!");

        CheckPuzzleComplete();
    }

    public void ClearSelectedCell()
    {
        if (!IsActive())
            return;

        if (selectedRow < 0 ||
            selectedColumn < 0)
        {
            return;
        }

        if (fixedCells[selectedRow, selectedColumn])
        {
            return;
        }

        playerGrid[selectedRow, selectedColumn] = 0;

        cells[selectedRow, selectedColumn]
            .SetNumber(0);

        cells[selectedRow, selectedColumn]
            .ClearPencilMarks();

        SetStatus("");
    }

    // =========================================================
    // PENCIL MODE
    // =========================================================

    private void OnPencilModeChanged(bool enabled)
    {
        if (enabled)
        {
            SetStatus("Pencil mode ON");
        }
        else
        {
            SetStatus("Pencil mode OFF");
        }
    }

    private void TogglePencilMark(int number)
    {
        if (cells[selectedRow, selectedColumn] == null)
            return;

        /*
         * Pencil marks are only useful when the cell
         * doesn't already contain a final answer.
         */

        if (playerGrid[selectedRow, selectedColumn] != 0)
        {
            SetStatus(
                "Clear the cell before using pencil marks."
            );

            return;
        }

        cells[selectedRow, selectedColumn]
            .TogglePencilMark(number);
    }

    // =========================================================
    // COMPLETION
    // =========================================================

    private void CheckPuzzleComplete()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int column = 0; column < gridSize; column++)
            {
                if (playerGrid[row, column] == 0)
                    return;

                if (playerGrid[row, column] !=
                    solution[row, column])
                {
                    return;
                }
            }
        }

        SetStatus("Sudoku solved!");

        PuzzleManager.Instance.EndPuzzle(
            PuzzleResult.Solved
        );
    }

    // =========================================================
    // UTILITY
    // =========================================================

    private void UpdateMistakeUI()
    {
        if (mistakeText != null)
        {
            mistakeText.text =
                $"Mistakes: {mistakes}";
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);

            T temp = list[i];

            list[i] = list[j];
            list[j] = temp;
        }
    }

    private int[,] CopyArray(int[,] source)
    {
        int rows = source.GetLength(0);
        int columns = source.GetLength(1);

        int[,] copy =
            new int[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                copy[row, column] =
                    source[row, column];
            }
        }

        return copy;
    }
}