using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JigsawPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private JigsawPuzzle puzzle;
    private Canvas canvas;

    public int TargetX { get; private set; }
    public int TargetY { get; private set; }

    private bool locked = false;

    public void Init(JigsawPuzzle puzzle, int x, int y)
    {
        this.puzzle = puzzle;
        TargetX = x;
        TargetY = y;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (locked) return;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling(); // bring to front
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (locked) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (locked) return;
        canvasGroup.blocksRaycasts = true;

        // Detect which cell is nearest under pointer
        JigsawCell nearest = null;
        float nearestDist = float.MaxValue;

        if (puzzle.Cells == null || puzzle.Cells.Count == 0)
        {
            Debug.LogWarning("No JigsawCells found — did the puzzle generate its grid?");
            return;
        }

        foreach (var cell in puzzle.Cells)
        {
            if (cell == null) continue;

            RectTransform cellRect = cell.GetComponent<RectTransform>();

            // Compare in world space
            Vector3 pieceWorld = rectTransform.TransformPoint(rectTransform.rect.center);
            Vector3 cellWorld = cellRect.TransformPoint(cellRect.rect.center);

            float dist = Vector2.Distance(pieceWorld, cellWorld);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = cell;
            }
        }

        if (nearest != null && nearestDist < 80f)
            puzzle.OnPiecePlaced(this, nearest);
    }

    public void LockPiece()
    {
        locked = true;
        GetComponent<Image>().raycastTarget = false;
    }
}
