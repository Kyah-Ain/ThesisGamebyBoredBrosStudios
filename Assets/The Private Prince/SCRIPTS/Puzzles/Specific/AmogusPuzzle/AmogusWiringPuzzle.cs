using System.Collections.Generic;
using UnityEngine;

public class AmogusWiringPuzzle : PuzzleBase
{
    private enum WiringState
    {
        Selecting,
        MovingPlug
    }

    [Header("Wire Count")]
    [Min(2)]
    [SerializeField] private int wireCount = 4;

    [Header("Containers")]
    [SerializeField] private RectTransform puzzleRoot;
    [SerializeField] private RectTransform leftContainer;
    [SerializeField] private RectTransform rightContainer;

    /*
     * Plug + dynamic line objects are moved here while playing.
     *
     * Make this stretch across the entire puzzle panel.
     */
    [SerializeField] private RectTransform dragLayer;

    [Header("Prefabs")]
    [SerializeField] private WiringLeftWire leftWirePrefab;
    [SerializeField] private WiringRightWire rightWirePrefab;

    [Header("Spacing")]
    [Tooltip(
        "Empty space between the first/last wire and container edges."
    )]
    [SerializeField] private float verticalMargin = 30f;

    [Header("Plug Movement")]
    [SerializeField] private float movementStep = 12f;

    [Header("Snapping")]
    [SerializeField] private float snapDistance = 25f;

    [Header("Wire Appearance")]
    [SerializeField]
    private List<Color> wireColors =
        new List<Color>();

    [SerializeField]
    private List<string> symbols =
        new List<string>();

    private readonly List<WiringLeftWire> leftWires =
        new List<WiringLeftWire>();

    private readonly List<WiringRightWire> rightWires =
        new List<WiringRightWire>();

    private WiringState currentState =
        WiringState.Selecting;

    private int selectedWireIndex;

    private WiringLeftWire movingWire;

    private bool puzzleGenerated;


    // =========================================================
    // PUZZLE LIFECYCLE
    // =========================================================

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        if (!puzzleGenerated)
            GeneratePuzzle();

        currentState = WiringState.Selecting;

        SelectFirstAvailableWire();
    }


    protected override void OnPuzzleReset()
    {
        ClearPuzzle();

        puzzleGenerated = false;
        movingWire = null;

        currentState = WiringState.Selecting;
    }


    // =========================================================
    // INPUT
    // =========================================================

    public override void HandleInput()
    {
        if (currentState == WiringState.Selecting)
        {
            HandleSelectionInput();
        }
        else
        {
            HandlePlugMovementInput();
        }
    }


    private void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        /*
         * RIGHT selects the current wire.
         */
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            BeginMovingSelectedWire();
        }
    }


    private void HandlePlugMovementInput()
    {
        if (movingWire == null)
            return;

        Vector2 movement = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            movement += Vector2.up;

        if (Input.GetKey(KeyCode.DownArrow))
            movement += Vector2.down;

        if (Input.GetKey(KeyCode.LeftArrow))
            movement += Vector2.left;

        if (Input.GetKey(KeyCode.RightArrow))
            movement += Vector2.right;

        if (movement != Vector2.zero)
        {
            movingWire.MovePlug(
                movement.normalized * movementStep
            );

            ClampPlugToPuzzle();

            CheckForSnap();
        }
    }


    // =========================================================
    // SELECTION
    // =========================================================

    private void MoveSelection(int direction)
    {
        if (leftWires.Count == 0)
            return;

        leftWires[selectedWireIndex].SetSelected(false);

        int attempts = 0;

        do
        {
            selectedWireIndex += direction;

            if (selectedWireIndex < 0)
                selectedWireIndex = leftWires.Count - 1;

            if (selectedWireIndex >= leftWires.Count)
                selectedWireIndex = 0;

            attempts++;

        }
        while (
            leftWires[selectedWireIndex].Connected &&
            attempts < leftWires.Count
        );

        leftWires[selectedWireIndex].SetSelected(true);
    }


    private void SelectFirstAvailableWire()
    {
        for (int i = 0; i < leftWires.Count; i++)
        {
            leftWires[i].SetSelected(false);

            if (!leftWires[i].Connected)
            {
                selectedWireIndex = i;
                leftWires[i].SetSelected(true);
                return;
            }
        }
    }


    private void BeginMovingSelectedWire()
    {
        if (selectedWireIndex < 0 ||
            selectedWireIndex >= leftWires.Count)
        {
            return;
        }

        WiringLeftWire selected =
            leftWires[selectedWireIndex];

        if (selected.Connected)
            return;

        movingWire = selected;

        movingWire.SetSelected(false);
        movingWire.BeginMoving();

        currentState = WiringState.MovingPlug;
    }


    // =========================================================
    // SNAP
    // =========================================================

    private void CheckForSnap()
    {
        if (movingWire == null)
            return;

        WiringRightWire target =
            FindMatchingRightWire(
                movingWire.PairID
            );

        if (target == null)
            return;

        float distance =
            Vector3.Distance(
                movingWire.Plug.position,
                target.SnapPoint.position
            );

        if (distance <= snapDistance)
        {
            CompleteConnection(
                movingWire,
                target
            );
        }
    }


    private WiringRightWire FindMatchingRightWire(int pairID)
    {
        foreach (WiringRightWire rightWire in rightWires)
        {
            if (rightWire.PairID == pairID)
                return rightWire;
        }

        return null;
    }


    private void CompleteConnection(
        WiringLeftWire left,
        WiringRightWire right)
    {
        left.SnapTo(right.SnapPoint);

        movingWire = null;

        if (AllWiresConnected())
        {
            PuzzleManager.Instance.EndPuzzle(
                PuzzleResult.Solved
            );

            return;
        }

        currentState = WiringState.Selecting;

        SelectFirstAvailableWire();
    }


    private bool AllWiresConnected()
    {
        foreach (WiringLeftWire wire in leftWires)
        {
            if (!wire.Connected)
                return false;
        }

        return true;
    }


    // =========================================================
    // GENERATION
    // =========================================================

    private void GeneratePuzzle()
    {
        ClearPuzzle();

        /*
         * Prevent impossible setup.
         */
        int count = Mathf.Min(
            wireCount,
            wireColors.Count,
            symbols.Count
        );

        if (count < 2)
        {
            Debug.LogError(
                "AmogusWiringPuzzle requires at least " +
                "2 colors and 2 symbols."
            );

            return;
        }

        List<int> leftOrder =
            CreateIndexList(count);

        List<int> rightOrder =
            CreateIndexList(count);

        /*
         * Right side should not have the exact same order.
         */
        Shuffle(rightOrder);

        /*
         * Create left side.
         */
        for (int i = 0; i < count; i++)
        {
            int pairID = leftOrder[i];

            WiringLeftWire wire =
                Instantiate(
                    leftWirePrefab,
                    leftContainer
                );

            ResetPrefabTransform(
                wire.GetComponent<RectTransform>()
            );

            wire.Initialize(
                pairID,
                wireColors[pairID],
                symbols[pairID],
                puzzleRoot,
                dragLayer
            );

            leftWires.Add(wire);
        }

        /*
         * Create right side.
         */
        for (int i = 0; i < count; i++)
        {
            int pairID = rightOrder[i];

            WiringRightWire wire =
                Instantiate(
                    rightWirePrefab,
                    rightContainer
                );

            ResetPrefabTransform(
                wire.GetComponent<RectTransform>()
            );

            wire.Initialize(
                pairID,
                wireColors[pairID],
                symbols[pairID]
            );

            rightWires.Add(wire);
        }

        PositionWires(leftWires, leftContainer);
        PositionWires(rightWires, rightContainer);

        /*
         * Positioning changed the PlugOrigin world positions,
         * so reset after everything is laid out.
         */
        foreach (WiringLeftWire wire in leftWires)
        {
            wire.ResetPlug();
        }

        puzzleGenerated = true;
    }


    // =========================================================
    // DYNAMIC SPACING
    // =========================================================

    private void PositionWires<T>(
        List<T> wires,
        RectTransform container)
        where T : MonoBehaviour
    {
        if (wires.Count == 0)
            return;

        float containerHeight =
            container.rect.height;

        RectTransform firstRect =
            wires[0].GetComponent<RectTransform>();

        /*
         * Keep the entire prefab inside the margins.
         */
        float halfItemHeight =
            firstRect.rect.height * 0.5f;

        float top =
            containerHeight * 0.5f
            - verticalMargin
            - halfItemHeight;

        float bottom =
            -containerHeight * 0.5f
            + verticalMargin
            + halfItemHeight;

        /*
         * One wire simply goes in the center.
         */
        if (wires.Count == 1)
        {
            RectTransform rect =
                wires[0].GetComponent<RectTransform>();

            rect.anchoredPosition =
                new Vector2(
                    rect.anchoredPosition.x,
                    0f
                );

            return;
        }

        float spacing =
            (top - bottom)
            / (wires.Count - 1);

        for (int i = 0; i < wires.Count; i++)
        {
            RectTransform rect =
                wires[i].GetComponent<RectTransform>();

            float y =
                top - spacing * i;

            rect.anchoredPosition =
                new Vector2(
                    rect.anchoredPosition.x,
                    y
                );
        }
    }


    private void ResetPrefabTransform(
        RectTransform rect)
    {
        /*
         * Fixed-size prefab.
         *
         * No stretching.
         */
        Vector2 originalSize =
            rect.sizeDelta;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        rect.sizeDelta = originalSize;
    }


    // =========================================================
    // BOUNDS
    // =========================================================

    private void ClampPlugToPuzzle()
    {
        if (movingWire == null)
            return;

        RectTransform plug =
            movingWire.Plug;

        Rect rect =
            puzzleRoot.rect;

        Vector2 position =
            plug.anchoredPosition;

        float halfWidth =
            plug.rect.width * 0.5f;

        float halfHeight =
            plug.rect.height * 0.5f;

        position.x = Mathf.Clamp(
            position.x,
            rect.xMin + halfWidth,
            rect.xMax - halfWidth
        );

        position.y = Mathf.Clamp(
            position.y,
            rect.yMin + halfHeight,
            rect.yMax - halfHeight
        );

        plug.anchoredPosition = position;

        movingWire.UpdateLine();
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private List<int> CreateIndexList(int count)
    {
        List<int> result =
            new List<int>();

        for (int i = 0; i < count; i++)
            result.Add(i);

        return result;
    }


    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            int temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }


    private void ClearPuzzle()
    {
        foreach (WiringLeftWire wire in leftWires)
        {
            if (wire != null)
                Destroy(wire.gameObject);
        }

        foreach (WiringRightWire wire in rightWires)
        {
            if (wire != null)
                Destroy(wire.gameObject);
        }

        leftWires.Clear();
        rightWires.Clear();

        /*
         * Dynamic line/plug objects may have been reparented
         * outside their prefab.
         *
         * Clear those too.
         */
        if (dragLayer != null)
        {
            for (
                int i = dragLayer.childCount - 1;
                i >= 0;
                i--)
            {
                Destroy(
                    dragLayer.GetChild(i).gameObject
                );
            }
        }
    }
}