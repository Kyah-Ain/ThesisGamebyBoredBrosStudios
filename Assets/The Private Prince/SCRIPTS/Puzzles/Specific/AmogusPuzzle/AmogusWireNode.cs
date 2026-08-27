using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AmogusWireNode : MonoBehaviour,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Wire Settings")]
    public int pairID;

    public bool isLeftNode;

    [HideInInspector]
    public bool connected;

    private Image image;
    private AmogusPuzzle puzzle;

    public RectTransform RectTransform { get; private set; }

    private Color wireColor;

    public Color WireColor => wireColor;

    [SerializeField]
    private RectTransform wireAnchor;


    public Vector2 WirePosition
    {
        get
        {
            RectTransform parent = puzzle.WireContainer as RectTransform;

            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                wireAnchor.position,
                null,
                out localPoint);

            Debug.Log($"{name}: {localPoint}");

            return localPoint;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();

        if (image == null)
            image = GetComponentInChildren<Image>();

        RectTransform = GetComponent<RectTransform>();
    }

    public void SetPuzzle(AmogusPuzzle owner)
    {
        puzzle = owner;
    }

    public void Initialize(int id, Color color, bool leftSide, AmogusPuzzle owner)
    {
        if (image == null)
            image = GetComponent<Image>();

        pairID = id;
        isLeftNode = leftSide;
        connected = false;

        puzzle = owner;

        wireColor = color;

        image.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!puzzle.IsActive())
            return;

        if (connected)
            return;

        if (!isLeftNode)
            return;

        puzzle.BeginConnection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        puzzle.CurrentHoverNode = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (puzzle.CurrentHoverNode == this)
            puzzle.CurrentHoverNode = null;
    }

    public bool Matches(AmogusWireNode other)
    {
        return other != null &&
               pairID == other.pairID;
    }

    public void Connect()
    {
        connected = true;
    }

    public void ResetNode()
    {
        connected = false;
    }
}