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
    public float thinLineWidth = 50f;
    public float thickLineWidth = 100f;

    [Header("Number Sprites")]
    [Tooltip("Index 0 = number 1, index 1 = number 2, etc.")]
    public Sprite[] numberSprites;

    [Header("Selection UI")]
    public TextMeshProUGUI mistakeText;

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

    private float cellSize;
    private float gridOffsetX;
    private float gridOffsetY;
    private float totalGridWidth;
    private float totalGridHeight;

    private float[] verticalLineX;
    private float[] verticalLineWidths;
    private float[] horizontalLineY;
    private float[] horizontalLineHeights;

    private int[,] solution;
    private int[,] playerGrid;
    private bool[,] prefilled;

    private SudokuCell[,] cells;
    private float gridPixelSize = 530f;

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

        UpdateGridPixelSize();
        ComputeLayout();

        // Lines first, so cells are created (and drawn) after them.
        GenerateGridLines();

        cellContainer = new GameObject(
    "Cells",
    typeof(RectTransform)
).GetComponent<RectTransform>();

        cellContainer.SetParent(gridRoot, false);

        // Same anchoring as the lines: top-left anchor and pivot,
        // with the container sitting exactly over the grid area.
        cellContainer.anchorMin = new Vector2(0, 1);
        cellContainer.anchorMax = new Vector2(0, 1);
        cellContainer.pivot = new Vector2(0, 1);
        cellContainer.sizeDelta = new Vector2(gridPixelSize, gridPixelSize);
        cellContainer.anchoredPosition = Vector2.zero;

        cells = new SudokuCell[gridSize, gridSize];

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                SudokuCell cell =
                    Instantiate(cellPrefab, cellContainer);

                RectTransform rect =
                    cell.GetComponent<RectTransform>();

                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;

                rect.sizeDelta = new Vector2(cellSize, cellSize);

                // Cell sits just after its leading line.
                float x = verticalLineX[col] + verticalLineWidths[col];
                float y = horizontalLineY[row] + horizontalLineHeights[row];

                rect.anchoredPosition = new Vector2(x, -y);

                cell.Setup(row, col, cellSize, numberSprites);

                cell.SetValue(playerGrid[row, col]);
                cell.SetPrefilled(prefilled[row, col]);

                cells[row, col] = cell;
            }
        }
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

    private void UpdateGridPixelSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRoot);

        float size = Mathf.Min(gridRoot.rect.width, gridRoot.rect.height);

        // Fallback if the hierarchy hasn't been laid out yet.
        gridPixelSize = size > 0f ? size : 530f;
    }

    // ---------------------------------------------------------
    // GRID LINES
    // ---------------------------------------------------------

    private void ComputeLayout()
    {
        verticalLineWidths = new float[gridSize + 1];
        horizontalLineHeights = new float[gridSize + 1];
        verticalLineX = new float[gridSize + 1];
        horizontalLineY = new float[gridSize + 1];

        float totalVertical = 0f;
        float totalHorizontal = 0f;

        for (int i = 0; i <= gridSize; i++)
        {
            verticalLineWidths[i] =
                (i % subgridColumns == 0) ? thickLineWidth : thinLineWidth;

            horizontalLineHeights[i] =
                (i % subgridRows == 0) ? thickLineWidth : thinLineWidth;

            totalVertical += verticalLineWidths[i];
            totalHorizontal += horizontalLineHeights[i];
        }

        // Cells are square, so use the more restrictive axis.
        cellSize = Mathf.Min(
            (gridPixelSize - totalVertical) / gridSize,
            (gridPixelSize - totalHorizontal) / gridSize
        );

        totalGridWidth = cellSize * gridSize + totalVertical;
        totalGridHeight = cellSize * gridSize + totalHorizontal;

        // Center the whole grid inside gridRoot.
        gridOffsetX = (gridPixelSize - totalGridWidth) * 0.5f;
        gridOffsetY = (gridPixelSize - totalGridHeight) * 0.5f;

        float runningX = gridOffsetX;
        float runningY = gridOffsetY;

        for (int i = 0; i <= gridSize; i++)
        {
            verticalLineX[i] = runningX;
            horizontalLineY[i] = runningY;

            runningX += verticalLineWidths[i] + cellSize;
            runningY += horizontalLineHeights[i] + cellSize;
        }

        Debug.Log($"gridPixelSize={gridPixelSize} cellSize={cellSize} " + 
            $"x[1]={verticalLineX[1]} y[1]={horizontalLineY[1]}");
    }

    private void GenerateGridLines()
    {
        for (int i = 0; i <= gridSize; i++)
        {
            CreateVerticalLine(i);
            CreateHorizontalLine(i);
        }
    }

    private void CreateVerticalLine(int index)
    {
        bool thick = index % subgridColumns == 0;

        Image prefab = thick ? thickLinePrefab : thinLinePrefab;

        if (prefab == null)
            return;

        Image line = Instantiate(prefab, gridRoot);

        RectTransform rect = line.rectTransform;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        rect.sizeDelta =
            new Vector2(verticalLineWidths[index], totalGridHeight);

        rect.anchoredPosition =
            new Vector2(verticalLineX[index], -gridOffsetY);
    }

    private void CreateHorizontalLine(int index)
    {
        bool thick = index % subgridRows == 0;

        Image prefab = thick ? thickLinePrefab : thinLinePrefab;

        if (prefab == null)
            return;

        Image line = Instantiate(prefab, gridRoot);

        RectTransform rect = line.rectTransform;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        rect.sizeDelta =
            new Vector2(totalGridWidth, horizontalLineHeights[index]);

        rect.anchoredPosition =
            new Vector2(gridOffsetX, -horizontalLineY[index]);
    }

    // ---------------------------------------------------------
    // INPUT
    // ---------------------------------------------------------

    public override void HandleInput()
    {
        if (!IsActive())
            return;

        if (inputMode == InputMode.SelectingCell)
            HandleCellSelection();
        else
            HandleNumberSelection();
    }

    private void HandleCellSelection()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveCell(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            MoveCell(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            MoveCell(0, -1);
        }
        else if (Input.GetKeyDown(KeyCode.D))
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
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S))
        {
            selectedNumber--;

            if (selectedNumber < 1)
                selectedNumber = gridSize;

            UpdateInputUI();
        }

        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.W))
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

        bool showSelection = inputMode == InputMode.SelectingCell;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                bool selected =
                    showSelection &&
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
        if (cells == null)
            return;

        SudokuCell cell = cells[selectedRow, selectedColumn];

        if (inputMode == InputMode.SelectingNumber)
            cell.SetPreview(selectedNumber);
        else
            cell.ClearPreview();

        UpdateSelection();
    }

    private void UpdateMistakeUI()
    {
        if (mistakeText == null || !useMistakeLimit)
            return;

        mistakeText.text = $"Mistakes: {mistakes}/{maxMistakes}";
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