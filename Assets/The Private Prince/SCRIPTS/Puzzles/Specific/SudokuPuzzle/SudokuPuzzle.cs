using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SudokuPuzzle : PuzzleBase
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    private enum InputMode
    {
        SelectingCell,
        SelectingNumber
    }

    [Header("Sudoku Settings")]
    public Difficulty difficulty = Difficulty.Easy;

    [Header("Puzzle UI")]
    public RectTransform gridRoot;
    public SudokuCell cellPrefab;

    [Header("Grid Lines")]
    public Image thinLinePrefab;
    public Image thickLinePrefab;

    [Header("Selection UI")]
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI selectedNumberText;
    public TextMeshProUGUI mistakeText;

    [Header("Number Selection Arrows")]
    public Image leftNumberArrow;
    public Image rightNumberArrow;

    [Header("Selected Number Colors")]
    public Color selectedNumberCellModeColor = Color.gray;
    public Color selectedNumberNumberModeColor = Color.white;

    [Header("Arrow Colors")]
    public Color arrowNormalColor = Color.gray;
    public Color arrowPressedColor = Color.white;

    [Header("Controls")]
    public KeyCode selectKey = KeyCode.Return;
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("Mistakes")]
    public bool useMistakeLimit = false;
    public int maxMistakes = 3;

    private int gridSize;
    private int subgridRows;
    private int subgridColumns;
    private int prefilledCount;

    private int[,] solution;
    private int[,] playerGrid;
    private bool[,] prefilled;

    private SudokuCell[,] cells;

    private int selectedRow = 0;
    private int selectedColumn = 0;
    private int selectedNumber = 1;

    private InputMode inputMode = InputMode.SelectingCell;

    private int mistakes = 0;

    private RectTransform cellContainer;

    // ---------------------------------------------------------
    // DIFFICULTY
    // ---------------------------------------------------------

    private void ConfigureDifficulty()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                gridSize = 3;
                subgridRows = 1;
                subgridColumns = 3;
                prefilledCount = 4;
                break;

            case Difficulty.Medium:
                gridSize = 4;
                subgridRows = 2;
                subgridColumns = 2;
                prefilledCount = 8;
                break;

            case Difficulty.Hard:
                gridSize = 6;
                subgridRows = 2;
                subgridColumns = 3;
                prefilledCount = 20;
                break;
        }
    }

    // ---------------------------------------------------------
    // START
    // ---------------------------------------------------------

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        ConfigureDifficulty();

        GeneratePuzzle();

        selectedRow = 0;
        selectedColumn = 0;
        selectedNumber = 1;
        inputMode = InputMode.SelectingCell;
        mistakes = 0;

        BuildGrid();

        UpdateSelection();
        UpdateInputUI();
        UpdateMistakeUI();
    }

    // ---------------------------------------------------------
    // GENERATION
    // ---------------------------------------------------------

    private void GeneratePuzzle()
    {
        solution = GenerateSolvedGrid();

        playerGrid = new int[gridSize, gridSize];
        prefilled = new bool[gridSize, gridSize];

        // Copy solution first.
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                playerGrid[row, col] = solution[row, col];
            }
        }

        // Remove all cells.
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                positions.Add(new Vector2Int(row, col));
            }
        }

        Shuffle(positions);

        int cellsToRemove = (gridSize * gridSize) - prefilledCount;

        for (int i = 0; i < cellsToRemove; i++)
        {
            Vector2Int position = positions[i];

            playerGrid[position.x, position.y] = 0;
        }

        // Mark remaining numbers as prefilled.
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                prefilled[row, col] = playerGrid[row, col] != 0;
            }
        }
    }

    private int[,] GenerateSolvedGrid()
    {
        int[,] grid = new int[gridSize, gridSize];

        FillGrid(grid);

        return grid;
    }

    private bool FillGrid(int[,] grid)
    {
        int row = -1;
        int col = -1;

        // Find empty cell.
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                if (grid[r, c] == 0)
                {
                    row = r;
                    col = c;
                    break;
                }
            }

            if (row != -1)
                break;
        }

        // No empty cells.
        if (row == -1)
            return true;

        List<int> numbers = new List<int>();

        for (int i = 1; i <= gridSize; i++)
            numbers.Add(i);

        Shuffle(numbers);

        foreach (int number in numbers)
        {
            if (!CanPlaceNumber(grid, row, col, number))
                continue;

            grid[row, col] = number;

            if (FillGrid(grid))
                return true;

            grid[row, col] = 0;
        }

        return false;
    }

    private bool CanPlaceNumber(
        int[,] grid,
        int row,
        int col,
        int number)
    {
        // Row
        for (int c = 0; c < gridSize; c++)
        {
            if (grid[row, c] == number)
                return false;
        }

        // Column
        for (int r = 0; r < gridSize; r++)
        {
            if (grid[r, col] == number)
                return false;
        }

        // Sub-grid
        int startRow = (row / subgridRows) * subgridRows;
        int startColumn = (col / subgridColumns) * subgridColumns;

        for (int r = startRow;
             r < startRow + subgridRows;
             r++)
        {
            for (int c = startColumn;
                 c < startColumn + subgridColumns;
                 c++)
            {
                if (grid[r, c] == number)
                    return false;
            }
        }

        return true;
    }

    // ---------------------------------------------------------
    // GRID UI
    // ---------------------------------------------------------

    private void BuildGrid()
    {
        ClearGrid();

        cellContainer = new GameObject(
            "Cells",
            typeof(RectTransform)
        ).GetComponent<RectTransform>();

        cellContainer.SetParent(gridRoot, false);

        cellContainer.anchorMin = Vector2.zero;
        cellContainer.anchorMax = Vector2.one;
        cellContainer.offsetMin = Vector2.zero;
        cellContainer.offsetMax = Vector2.zero;

        GridLayoutGroup layout =
            cellContainer.gameObject.AddComponent<GridLayoutGroup>();

        layout.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        layout.constraintCount = gridSize;

        float cellSize = 600f / gridSize;

        layout.cellSize = new Vector2(cellSize, cellSize);
        layout.spacing = Vector2.zero;
        layout.padding = new RectOffset(0, 0, 0, 0);

        cells = new SudokuCell[gridSize, gridSize];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                SudokuCell cell =
                    Instantiate(cellPrefab, cellContainer);

                cell.Setup(
                    row,
                    col,
                    cellSize
                );

                cell.SetValue(playerGrid[row, col]);
                cell.SetPrefilled(prefilled[row, col]);

                cells[row, col] = cell;
            }
        }

        GenerateGridLines();
    }

    private void ClearGrid()
    {
        if (cellContainer != null)
        {
            Destroy(cellContainer.gameObject);
            cellContainer = null;
        }

        // Remove old lines.
        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(gridRoot.GetChild(i).gameObject);
        }
    }

    // ---------------------------------------------------------
    // GRID LINES
    // ---------------------------------------------------------

    private void GenerateGridLines()
    {
        float cellSize = 600f / gridSize;

        // Vertical lines
        for (int column = 0; column <= gridSize; column++)
        {
            bool thick =
                column % subgridColumns == 0;

            CreateVerticalLine(
                column * cellSize,
                thick,
                cellSize
            );
        }

        // Horizontal lines
        for (int row = 0; row <= gridSize; row++)
        {
            bool thick =
                row % subgridRows == 0;

            CreateHorizontalLine(
                row * cellSize,
                thick,
                cellSize
            );
        }
    }

    private void CreateVerticalLine(
        float x,
        bool thick,
        float cellSize)
    {
        Image prefab =
            thick ? thickLinePrefab : thinLinePrefab;

        if (prefab == null)
            return;

        Image line = Instantiate(prefab, gridRoot);

        RectTransform rect = line.rectTransform;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);

        rect.pivot = new Vector2(0.5f, 1);

        float width =
            thick ? 5f : 1.5f;

        rect.sizeDelta =
            new Vector2(width, 600f);

        rect.anchoredPosition =
            new Vector2(x, 0);
    }

    private void CreateHorizontalLine(
        float y,
        bool thick,
        float cellSize)
    {
        Image prefab =
            thick ? thickLinePrefab : thinLinePrefab;

        if (prefab == null)
            return;

        Image line = Instantiate(prefab, gridRoot);

        RectTransform rect = line.rectTransform;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);

        rect.pivot = new Vector2(0, 0.5f);

        float height =
            thick ? 5f : 1.5f;

        rect.sizeDelta =
            new Vector2(600f, height);

        rect.anchoredPosition =
            new Vector2(0, -y);
    }

    // ---------------------------------------------------------
    // INPUT
    // ---------------------------------------------------------

    public override void HandleInput()
    {
        if (!IsActive())
            return;

        if (inputMode == InputMode.SelectingCell)
        {
            HandleCellSelection();
        }
        else
        {
            HandleNumberSelection();
        }

        UpdateArrowVisuals();
    }

    private void HandleCellSelection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveCell(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCell(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveCell(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveCell(0, 1);
        }

        if (Input.GetKeyDown(selectKey))
        {
            if (prefilled[selectedRow, selectedColumn])
                return;

            inputMode = InputMode.SelectingNumber;

            UpdateInputUI();
        }
    }

    private void HandleNumberSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedNumber--;

            if (selectedNumber < 1)
                selectedNumber = gridSize;

            UpdateInputUI();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedNumber++;

            if (selectedNumber > gridSize)
                selectedNumber = 1;

            UpdateInputUI();
        }

        if (Input.GetKeyDown(selectKey))
        {
            PlaceNumber(selectedNumber);
        }

        if (Input.GetKeyDown(cancelKey))
        {
            inputMode = InputMode.SelectingCell;

            UpdateInputUI();
        }
    }

    // ---------------------------------------------------------
    // CELL MOVEMENT
    // ---------------------------------------------------------

    private void MoveCell(int rowDirection, int columnDirection)
    {
        selectedRow += rowDirection;
        selectedColumn += columnDirection;

        selectedRow =
            Mathf.Clamp(selectedRow, 0, gridSize - 1);

        selectedColumn =
            Mathf.Clamp(selectedColumn, 0, gridSize - 1);

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (cells == null)
            return;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                bool selected =
                    row == selectedRow &&
                    col == selectedColumn;

                cells[row, col].SetSelected(selected);
            }
        }
    }

    // ---------------------------------------------------------
    // NUMBER PLACEMENT
    // ---------------------------------------------------------

    private void PlaceNumber(int number)
    {
        if (prefilled[selectedRow, selectedColumn])
            return;

        playerGrid[selectedRow, selectedColumn] = number;

        SudokuCell cell =
            cells[selectedRow, selectedColumn];

        cell.SetValue(number);

        if (number != solution[selectedRow, selectedColumn])
        {
            cell.SetWrong(true);

            mistakes++;

            UpdateMistakeUI();

            if (useMistakeLimit &&
                mistakes >= maxMistakes)
            {
                PuzzleManager.Instance.EndPuzzle(
                    PuzzleResult.Failed
                );

                return;
            }
        }
        else
        {
            cell.SetWrong(false);

            if (CheckSolved())
            {
                PuzzleManager.Instance.EndPuzzle(
                    PuzzleResult.Solved
                );

                return;
            }
        }

        inputMode = InputMode.SelectingCell;

        UpdateInputUI();
    }

    private bool CheckSolved()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                if (playerGrid[row, col] !=
                    solution[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------

    private void UpdateInputUI()
    {
        bool numberMode =
            inputMode == InputMode.SelectingNumber;

        // ---------------------------------------------------------
        // MODE TEXT
        // ---------------------------------------------------------

        if (modeText != null)
        {
            if (numberMode)
            {
                modeText.text =
                    "Select Number\nArrows + Enter";
            }
            else
            {
                modeText.text =
                    "Select Cell\nArrow Keys + Enter";
            }
        }

        // ---------------------------------------------------------
        // SELECTED NUMBER
        // ---------------------------------------------------------

        if (selectedNumberText != null)
        {
            selectedNumberText.text =
                selectedNumber.ToString();

            selectedNumberText.color =
                numberMode
                    ? selectedNumberNumberModeColor
                    : selectedNumberCellModeColor;
        }

        // ---------------------------------------------------------
        // ARROWS
        // ---------------------------------------------------------

        if (leftNumberArrow != null)
            leftNumberArrow.gameObject.SetActive(numberMode);

        if (rightNumberArrow != null)
            rightNumberArrow.gameObject.SetActive(numberMode);

        UpdateArrowVisuals();
    }

    private void UpdateMistakeUI()
    {
        if (mistakeText == null || !useMistakeLimit)
            return;

        mistakeText.text = $"Mistakes: {mistakes}/{maxMistakes}";
    }

    private void UpdateArrowVisuals()
    {
        if (inputMode != InputMode.SelectingNumber)
            return;

        bool leftPressed =
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.DownArrow);

        bool rightPressed =
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.UpArrow);

        if (leftNumberArrow != null)
        {
            leftNumberArrow.color =
                leftPressed
                    ? arrowPressedColor
                    : arrowNormalColor;
        }

        if (rightNumberArrow != null)
        {
            rightNumberArrow.color =
                rightPressed
                    ? arrowPressedColor
                    : arrowNormalColor;
        }
    }

    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------

    protected override void OnPuzzleReset()
    {
        ConfigureDifficulty();

        GeneratePuzzle();

        selectedRow = 0;
        selectedColumn = 0;
        selectedNumber = 1;

        inputMode = InputMode.SelectingCell;

        mistakes = 0;

        BuildGrid();

        UpdateSelection();
        UpdateInputUI();
        UpdateMistakeUI();
    }

    // ---------------------------------------------------------
    // UTILITY
    // ---------------------------------------------------------

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            T temp = list[i];

            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}