using UnityEngine;

public class AmogusWireDrag : MonoBehaviour
{
    public RectTransform dragArea;

    public Vector3 MousePosition
    {
        get
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragArea,
                Input.mousePosition,
                null,
                out Vector2 point);

            return dragArea.TransformPoint(point);
        }
    }
}