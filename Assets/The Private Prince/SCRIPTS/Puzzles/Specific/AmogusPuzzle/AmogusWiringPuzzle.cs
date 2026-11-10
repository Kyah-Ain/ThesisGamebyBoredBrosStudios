using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class AmogusWiringPuzzle : PuzzleBase
{
    private enum WiringState
    {
        Selecting,
        MovingPlug,
        Resetting
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

    //[Header("Keys")]
    //[Tooltip("Selects a wire to move. Kept configurable in case " +
    //         "console bindings are added later.")]
    // [SerializeField] private KeyCode interactKey = KeyCode.E;

    // [SerializeField] private KeyCode restartKey = KeyCode.R;

    [Header("Wire Appearance")]
    [SerializeField]
    private List<Color> wireColors =
        new List<Color>();

    [SerializeField]
    private List<Sprite> symbols =
        new List<Sprite>();

    [Header("Failure Reset")]
    [Tooltip("Pause before the puzzle resets when every wire is plugged " +
         "in but at least one is wrong.")]
    [SerializeField] private float wrongResetDelay = 0.6f;
    private Coroutine resetRoutine;

    private readonly List<WiringLeftWire> leftWires =
        new List<WiringLeftWire>();

    private readonly List<WiringRightWire> rightWires =
        new List<WiringRightWire>();

    private WiringState currentState =
        WiringState.Selecting;

    private int selectedWireIndex;

    private WiringLeftWire movingWire;

    /*
     * When a wrongly connected wire is picked back up, its plug
     * starts out sitting on that right wire's snap point. Ignore
     * that right wire until the plug has moved away from it,
     * otherwise it would instantly re-snap.
     */
    private WiringRightWire releasedFrom;

    private bool puzzleGenerated;


    // =========================================================
    // PUZZLE LIFECYCLE
    // =========================================================

    public override void StartPuzzle()
    {
        base.StartPuzzle();
        puzzleActive = true;
        navigateInput = Vector2.zero;
        
        if (!puzzleGenerated)
            GeneratePuzzle();

        currentState = WiringState.Selecting;

        SelectFirstAvailableWire();
    }


    protected override void OnPuzzleReset()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        ClearPuzzle();

        puzzleActive = false;
        
        puzzleGenerated = false;
        movingWire = null;
        releasedFrom = null;

        currentState = WiringState.Selecting;
    }


    /*
     * Restart: every wire goes back to its start, nothing is
     * connected. Layout and pairings stay the same.
     */
    private void RestartPuzzle()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        movingWire = null;
        releasedFrom = null;

        foreach (WiringRightWire right in rightWires)
        {
            right.ClearOccupant();
        }

        foreach (WiringLeftWire left in leftWires)
        {
            left.ResetWire();
        }

        currentState = WiringState.Selecting;

        SelectFirstAvailableWire();
    }
    
    // =========================================================
    // INPUT
    // =========================================================

    private const float NavigateThreshold = 0.5f;

    // Latest value from the Navigate action (zero when released)
    private Vector2 navigateInput;

    // Only react to input while this puzzle is actually being played
    private bool puzzleActive;

    public virtual void OnEnable()  { Subscribe(); }
    void OnDisable()                { Unsubscribe(); }

    // ----------------------- SUBSCRIPTION METHODS -------------------------

    // Method to subscribe to events as a listener
    public void Subscribe()
    {
        // Proceeds only if the Input Control manager was found
        if (GameEventsManager.Instance == null) return;

        // SUBSCRIBE METHODS to the input action events
        var input = GameEventsManager.Instance.inputEvents;
        input.onNavigate += OnNavigate;
        input.onPuzzleSubmit += OnPuzzleSubmit;
        input.onRestart  += OnRestart;
    }

    // Method to unsubscribe from events 
    public void Unsubscribe()
    {
        // A "canceled" event will never arrive once we're unsubscribed,
        // so clear the stored value to avoid a stuck direction.
        navigateInput = Vector2.zero;
        
        // Proceeds only if the Input Control manager was found
        if (GameEventsManager.Instance == null) return;

        // UNSUBSCRIBE METHODS to the input action events
        var input = GameEventsManager.Instance.inputEvents;
        input.onNavigate -= OnNavigate;
        input.onPuzzleSubmit -= OnPuzzleSubmit;
        input.onRestart  -= OnRestart;
    }

    #region AIN's NEW INPUTS

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        Vector2 previous = navigateInput;
        navigateInput = ctx.ReadValue<Vector2>(); // zero on canceled

        if (!puzzleActive || currentState != WiringState.Selecting)
            return;

        // Step only when crossing the threshold, not every time the value
        // changes. This stops a stick from firing many steps in one push.
        if (navigateInput.y > NavigateThreshold &&
            previous.y <= NavigateThreshold)
        {
            MoveSelection(-1);   // up
        }
        else if (navigateInput.y < -NavigateThreshold &&
                 previous.y >= -NavigateThreshold)
        {
            MoveSelection(1);    // down
        }
    }

    private void OnPuzzleSubmit(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive || currentState != WiringState.Selecting)
            return;

        BeginMovingSelectedWire();
    }

    private void OnRestart(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive) return;

        RestartPuzzle();
    }

    // Called every frame by your PuzzleManager. Only continuous
    // plug movement needs polling now.
    public override void HandleInput()
    {
        if (!puzzleActive ||
            currentState != WiringState.MovingPlug ||
            movingWire == null ||
            navigateInput == Vector2.zero)
        {
            return;
        }

        movingWire.MovePlug(
            Vector2.ClampMagnitude(navigateInput, 1f) * movementStep
        );

        ClampPlugToPuzzle();
        CheckForSnap();
    }

    #endregion

    #region PAGBILAO's OLD INPUTS

    // public override void HandleInput()
    // {
    //     if (Input.GetKeyDown(restartKey))
    //     {
    //         RestartPuzzle();
    //         return;
    //     }
    //
    //     if (currentState == WiringState.Selecting)
    //     {
    //         HandleSelectionInput();
    //     }
    //     else
    //     {
    //         HandlePlugMovementInput();
    //     }
    // }
    //
    // private void HandleSelectionInput()
    // {
    //     if (Input.GetKeyDown(KeyCode.W))
    //     {
    //         MoveSelection(-1);
    //     }
    //
    //     if (Input.GetKeyDown(KeyCode.S))
    //     {
    //         MoveSelection(1);
    //     }
    //
    //     if (Input.GetKeyDown(interactKey))
    //     {
    //         BeginMovingSelectedWire();
    //     }
    // }
    //
    // private void HandlePlugMovementInput()
    // {
    //     if (movingWire == null)
    //         return;
    //
    //     Vector2 movement = Vector2.zero;
    //
    //     if (Input.GetKey(KeyCode.W))
    //         movement += Vector2.up;
    //
    //     if (Input.GetKey(KeyCode.S))
    //         movement += Vector2.down;
    //
    //     if (Input.GetKey(KeyCode.A))
    //         movement += Vector2.left;
    //
    //     if (Input.GetKey(KeyCode.D))
    //         movement += Vector2.right;
    //
    //     if (movement != Vector2.zero)
    //     {
    //         movingWire.MovePlug(
    //             movement.normalized * movementStep
    //         );
    //
    //         ClampPlugToPuzzle();
    //
    //         CheckForSnap();
    //     }
    // }
    
    #endregion

    // =========================================================
    // SELECTION
    // =========================================================

    private void MoveSelection(int direction)
    {
        if (leftWires.Count == 0)
            return;

        leftWires[selectedWireIndex].SetSelected(false);

        int attempts = 0;

        /*
         * Skip only correctly connected wires. Wrongly connected
         * wires stay selectable so they can be retried.
         */
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
            leftWires[selectedWireIndex].Solved &&
            attempts < leftWires.Count
        );

        leftWires[selectedWireIndex].SetSelected(true);
    }


    private void SelectFirstAvailableWire()
    {
        for (int i = 0; i < leftWires.Count; i++)
        {
            leftWires[i].SetSelected(false);
        }

        for (int i = 0; i < leftWires.Count; i++)
        {
            if (!leftWires[i].Solved)
            {
                SelectWire(i);
                return;
            }
        }
    }


    private void SelectWire(int index)
    {
        if (index < 0 || index >= leftWires.Count)
            return;

        foreach (WiringLeftWire wire in leftWires)
        {
            wire.SetSelected(false);
        }

        selectedWireIndex = index;
        leftWires[index].SetSelected(true);
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

        if (selected.Solved)
            return;

        /*
         * Picking up a wrongly connected wire frees the right
         * wire it was sitting on.
         */
        releasedFrom = selected.PluggedInto;
        selected.Unplug();

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

        Vector3 plugPosition =
            movingWire.Plug.position;

        if (releasedFrom != null)
        {
            float releasedDistance =
                Vector3.Distance(
                    plugPosition,
                    releasedFrom.SnapPoint.position
                );

            if (releasedDistance > snapDistance)
                releasedFrom = null;
        }

        /*
         * Snap to the closest FREE right wire in range,
         * whether or not it is the matching one.
         */
        WiringRightWire closest = null;
        float closestDistance = snapDistance;

        foreach (WiringRightWire rightWire in rightWires)
        {
            if (rightWire.IsOccupied)
                continue;

            if (rightWire == releasedFrom)
                continue;

            float distance =
                Vector3.Distance(
                    plugPosition,
                    rightWire.SnapPoint.position
                );

            if (distance <= closestDistance)
            {
                closest = rightWire;
                closestDistance = distance;
            }
        }

        if (closest != null)
        {
            CompleteConnection(
                movingWire,
                closest
            );
        }
    }


    private void CompleteConnection(
        WiringLeftWire left,
        WiringRightWire right)
    {
        bool correct =
            left.PairID == right.PairID;

        left.SnapTo(right, correct);

        int snappedIndex =
            leftWires.IndexOf(left);

        movingWire = null;
        releasedFrom = null;

        if (correct && AllWiresSolved())
        {
            PuzzleManager.Instance.EndPuzzle(
                PuzzleResult.Solved
            );

            return;
        }

        if (AllWiresPlugged())
        {
            currentState = WiringState.Resetting;
            resetRoutine = StartCoroutine(ResetAfterDelay());
            return;
        }

        currentState = WiringState.Selecting;

        if (correct)
        {
            SelectFirstAvailableWire();
        }
        else
        {
            /*
             * Wrong pair: keep that wire selected so the player
             * can immediately pick it back up and retry.
             */
            SelectWire(snappedIndex);
        }
    }

    private bool AllWiresPlugged()
    {
        foreach (WiringLeftWire wire in leftWires)
        {
            if (wire.PluggedInto == null)
                return false;
        }

        return true;
    }


    private IEnumerator ResetAfterDelay()
    {
        // Realtime so it still works if the game is paused while
        // the puzzle is open.
        yield return new WaitForSecondsRealtime(wrongResetDelay);

        resetRoutine = null;
        ResetWrongWires();
    }


    /*
     * Partial reset: wrongly plugged wires go back to their start
     * and free up their right wires. Correct connections stay.
     */
    private void ResetWrongWires()
    {
        movingWire = null;
        releasedFrom = null;

        foreach (WiringLeftWire left in leftWires)
        {
            if (!left.Solved)
                left.ResetWire();
        }

        currentState = WiringState.Selecting;

        SelectFirstAvailableWire();
    }

    private bool AllWiresSolved()
    {
        foreach (WiringLeftWire wire in leftWires)
        {
            if (!wire.Solved)
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