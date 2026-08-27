using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(GridLayoutGroup))]
public class SudokuGridLayout : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private RectTransform gridRect;

    [Header("Cell Settings")]
    [SerializeField] private float spacing = 2f;

    [Header("Subgrid Lines")]
    [SerializeField] private float subgridLineThickness = 4f;

    [SerializeField] private Color subgridLineColor = Color.black;

    private GridLayoutGroup grid;

    private readonly List<GameObject> subgridLines =
        new List<GameObject>();

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();

        if (gridRect == null)
        {
            gridRect = GetComponent<RectTransform>();
        }
    }

    public void SetGridSize(int size)
    {
        if (grid == null)
        {
            grid = GetComponent<GridLayoutGroup>();
        }

        if (gridRect == null)
        {
            gridRect = GetComponent<RectTransform>();
        }

        /*
         * Configure GridLayoutGroup.
         */

        grid.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        grid.constraintCount = size;

        grid.spacing =
            new Vector2(spacing, spacing);

        /*
         * Calculate cell size.
         */

        float width = gridRect.rect.width;
        float height = gridRect.rect.height;

        float totalSpacing =
            spacing * (size - 1);

        float cellWidth =
            (width - totalSpacing) / size;

        float cellHeight =
            (height - totalSpacing) / size;

        float cellSize =
            Mathf.Min(cellWidth, cellHeight);

        grid.cellSize =
            new Vector2(cellSize, cellSize);

        /*
         * Configure the Sudoku regions.
         */

        int regionRows;
        int regionColumns;

        GetRegionSize(
            size,
            out regionRows,
            out regionColumns
        );

        /*
         * Rebuild subgrid lines.
         */

        ClearSubgridLines();

        CreateSubgridLines(
            size,
            regionRows,
            regionColumns
        );
    }

    private void GetRegionSize(
        int size,
        out int regionRows,
        out int regionColumns)
    {
        switch (size)
        {
            case 4:

                // 2 × 2 regions
                regionRows = 2;
                regionColumns = 2;

                break;

            case 6:

                // 2 × 3 regions
                regionRows = 2;
                regionColumns = 3;

                break;

            case 9:

                // 3 × 3 regions
                regionRows = 3;
                regionColumns = 3;

                break;

            default:

                regionRows = 1;
                regionColumns = 1;

                break;
        }
    }

    private void CreateSubgridLines(
        int size,
        int regionRows,
        int regionColumns)
    {
        /*
         * Horizontal boundaries.
         */

        for (
            int row = regionRows;
            row < size;
            row += regionRows)
        {
            CreateHorizontalLine(
                row,
                size
            );
        }

        /*
         * Vertical boundaries.
         */

        for (
            int column = regionColumns;
            column < size;
            column += regionColumns)
        {
            CreateVerticalLine(
                column,
                size
            );
        }
    }

    private void CreateHorizontalLine(
        int row,
        int size)
    {
        GameObject line =
            CreateLineObject("HorizontalLine");

        RectTransform rect =
            line.GetComponent<RectTransform>();

        float cellSize =
            grid.cellSize.x;

        float totalWidth =
            cellSize * size +
            spacing * (size - 1);

        float y =
            -(cellSize + spacing) * row +
            spacing / 2f;

        rect.anchorMin =
            new Vector2(0.5f, 1f);

        rect.anchorMax =
            new Vector2(0.5f, 1f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(
                totalWidth,
                subgridLineThickness
            );

        rect.anchoredPosition =
            new Vector2(
                0,
                y
            );
    }

    private void CreateVerticalLine(
        int column,
        int size)
    {
        GameObject line =
            CreateLineObject("VerticalLine");

        RectTransform rect =
            line.GetComponent<RectTransform>();

        float cellSize =
            grid.cellSize.x;

        float totalHeight =
            cellSize * size +
            spacing * (size - 1);

        float x =
            (cellSize + spacing) * column -
            spacing / 2f;

        rect.anchorMin =
            new Vector2(0f, 0.5f);

        rect.anchorMax =
            new Vector2(0f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(
                subgridLineThickness,
                totalHeight
            );

        rect.anchoredPosition =
            new Vector2(
                x,
                0
            );
    }

    private GameObject CreateLineObject(
        string lineName)
    {
        GameObject line =
            new GameObject(lineName);

        line.transform.SetParent(
            transform,
            false
        );

        Image image =
            line.AddComponent<Image>();

        image.color =
            subgridLineColor;

        subgridLines.Add(line);

        return line;
    }

    private void ClearSubgridLines()
    {
        foreach (GameObject line in subgridLines)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }

        subgridLines.Clear();
    }
}