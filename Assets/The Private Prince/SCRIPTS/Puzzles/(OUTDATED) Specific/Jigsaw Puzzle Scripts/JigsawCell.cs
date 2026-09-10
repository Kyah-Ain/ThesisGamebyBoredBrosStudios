using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JigsawCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    private Color originalColor;
    private JigsawPuzzle puzzle;
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void Init(JigsawPuzzle puzzle, int x, int y)
    {
        this.puzzle = puzzle;
        GridX = x;
        GridY = y;
        image = GetComponent<Image>();
        originalColor = image.color;
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f); // semi-transparent
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
    }
}
