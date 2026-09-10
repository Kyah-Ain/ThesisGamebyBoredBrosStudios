using UnityEngine;
using UnityEngine.UI;

public class WireSegment : MonoBehaviour
{
    [SerializeField] private Image lineImage;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Vector2 start, Vector2 end, Color color, float thickness = 8f)
    {
        // Position at midpoint
        Vector2 midpoint = (start + end) * 0.5f;
        rectTransform.anchoredPosition = midpoint;

        // Calculate rotation and length
        Vector2 direction = end - start;
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set size and rotation
        rectTransform.sizeDelta = new Vector2(length, thickness);
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        // Set color
        lineImage.color = color;
    }
}