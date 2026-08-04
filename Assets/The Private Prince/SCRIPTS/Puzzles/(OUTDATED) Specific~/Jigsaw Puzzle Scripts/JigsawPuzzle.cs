using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class JigsawPuzzle : PuzzleBase
{
    [Header("Jigsaw Settings")]
    public Sprite sourceImage;                     // The full image to split
    public int rows = 3;
    public int cols = 3;
    public int errorLimit = 0;                     // 0 = unlimited

    [Header("Jigsaw References")]
    public RectTransform referenceArea;            // The 600x600 area on the right
    public RectTransform pieceContainer;           // The 730x600 area on the left
    public TextMeshProUGUI errorText;              // Shows number of mistakes
    public GameObject piecePrefab;                 // A prefab with Image + Drag handler
    public GameObject cellPrefab;                  // A prefab with Image (acts as cell target)

    private List<JigsawPiece> pieces = new List<JigsawPiece>();
    private List<JigsawCell> cells = new List<JigsawCell>();
    private int totalPieces;
    private int placedPieces = 0;
    private int errorCount = 0;
    private bool initialized = false;

    public IReadOnlyList<JigsawCell> Cells => cells;

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        if (!initialized)
        {
            GenerateGrid();
            GeneratePieces();
            initialized = true;
        }

        UpdateErrorText();
    }

    public override void EndPuzzle(PuzzleResult result)
    {
        base.EndPuzzle(result);
        Debug.Log($"Jigsaw puzzle ended with {result}");
    }

    private void GenerateGrid()
    {
        // Clear previous cells if any
        foreach (Transform child in referenceArea)
            Destroy(child.gameObject);
        cells.Clear();

        float cellWidth = referenceArea.rect.width / cols;
        float cellHeight = referenceArea.rect.height / rows;

        for (int y = 0; y < rows; y++)
        {
            int flippedY = (rows - 1) - y; // Flip Y to match texture coords
            for (int x = 0; x < cols; x++)
            {
                GameObject cellGO = Instantiate(cellPrefab, referenceArea);
                RectTransform rt = cellGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(cellWidth, cellHeight);
                rt.anchoredPosition = new Vector2(
                    (x * cellWidth) - referenceArea.rect.width / 2 + cellWidth / 2,
                    (-flippedY * cellHeight) + referenceArea.rect.height / 2 - cellHeight / 2
                );

                JigsawCell cell = cellGO.AddComponent<JigsawCell>();
                cell.Init(this, x, y);
                cells.Add(cell);
            }
        }
    }

    private void GeneratePieces()
    {
        foreach (Transform child in pieceContainer)
            Destroy(child.gameObject);
        pieces.Clear();

        Texture2D tex = sourceImage.texture;
        float pieceWidth = tex.width / cols;
        float pieceHeight = tex.height / rows;
        totalPieces = rows * cols;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Extract subtexture
                Texture2D subTex = new Texture2D((int)pieceWidth, (int)pieceHeight);
                subTex.SetPixels(tex.GetPixels(
                    (int)(x * pieceWidth),
                    (int)(y * pieceHeight),
                    (int)pieceWidth,
                    (int)pieceHeight
                ));
                subTex.Apply();

                Sprite subSprite = Sprite.Create(subTex,
                    new Rect(0, 0, pieceWidth, pieceHeight),
                    new Vector2(0.5f, 0.5f));

                GameObject pieceGO = Instantiate(piecePrefab, pieceContainer);
                Image img = pieceGO.GetComponent<Image>();
                img.sprite = subSprite;

                RectTransform rt = pieceGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(referenceArea.rect.width / cols, referenceArea.rect.height / rows);

                // Randomize position within container
                rt.anchoredPosition = new Vector2(
                    Random.Range(-pieceContainer.rect.width / 2 + 100, pieceContainer.rect.width / 2 - 100),
                    Random.Range(-pieceContainer.rect.height / 2 + 100, pieceContainer.rect.height / 2 - 100)
                );

                JigsawPiece piece = pieceGO.AddComponent<JigsawPiece>();
                piece.Init(this, x, y);
                pieces.Add(piece);
            }
        }
    }

    public void OnPiecePlaced(JigsawPiece piece, JigsawCell cell)
    {
        if (cell.GridX == piece.TargetX && cell.GridY == piece.TargetY)
        {
            // Correct placement
            piece.transform.SetParent(referenceArea);
            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            RectTransform cellRect = cell.GetComponent<RectTransform>();

            // Convert cell position into the piece's new parent's local space
            Vector3 worldPos = cellRect.TransformPoint(cellRect.rect.center);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                referenceArea,
                RectTransformUtility.WorldToScreenPoint(null, worldPos),
                null,
                out localPos
            );
            pieceRect.anchoredPosition = localPos;
            piece.LockPiece();
            placedPieces++;

            if (placedPieces >= totalPieces)
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
            }
        }
        else
        {
            // Wrong placement
            errorCount++;
            UpdateErrorText();

            if (errorLimit > 0 && errorCount >= errorLimit)
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
            }
        }
    }

    private void UpdateErrorText()
    {
        if (errorText == null) return;

        if (errorLimit <= 0)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
        else
        {
            errorText.gameObject.SetActive(true);
            errorText.text = $"Errors: {errorCount}/{errorLimit}";
        }
    }

    public override void HandleInput()
    {
        // Nothing specific here — handled by Drag events in JigsawPiece
    }

    protected override void OnPuzzleReset()
    {
        // Reset all progress-related counters
        placedPieces = 0;
        errorCount = 0;

        // Clear existing pieces and cells
        foreach (Transform child in referenceArea)
            Destroy(child.gameObject);
        foreach (Transform child in pieceContainer)
            Destroy(child.gameObject);

        pieces.Clear();
        cells.Clear();

        // Update UI
        UpdateErrorText();

        initialized = false;
    }
}
