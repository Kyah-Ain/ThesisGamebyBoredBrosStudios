using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class WirePair
{
    public WireBase startBase;
    public WireBase endBase;
    public Color wireColor;
    public bool isConnected = false;
}

[System.Serializable]
public class GlitchDot
{
    public WireBase glitchBase;
    public Color glitchColor;
    public Vector2Int gridPosition;
}

public class ConnectPuzzle : PuzzleBase
{
    [Header("Connect Puzzle Setup")]
    public int gridSize = 5;
    public float gridWidth = 700f;
    public GameObject wireBasePrefab;
    public GameObject wirePathPrefab;
    public GameObject wireSegmentPrefab;
    public RectTransform gridContainer;

    [Header("Wire Pairs")]
    public List<WirePairData> wirePairsData = new List<WirePairData>();

    [Header("Glitch Dots")]
    public List<GlitchDotData> glitchDotsData = new List<GlitchDotData>();

    private RectTransform puzzleContainer;
    private float cellSize;
    private WirePath[,] wirePaths;
    private List<WirePair> wirePairs = new List<WirePair>();
    private List<GlitchDot> glitchDots = new List<GlitchDot>();
    private WireBase hoveredWireBase = null;
    private WireBase currentlyDragging = null;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private List<WireSegment> currentWireSegments = new List<WireSegment>();
    private bool isDragging = false;

    private bool isCompleted = false;

    [System.Serializable]
    public class WirePairData
    {
        public Vector2Int startPosition;
        public Vector2Int endPosition;
        public Color wireColor;
    }

    [System.Serializable]
    public class GlitchDotData
    {
        public Vector2Int position;
        public Color glitchColor;
    }

    public override void HandleInput()
    {
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (isDragging && Input.GetMouseButton(0))
        {
            HandleMouseDrag();
        }
        else if (isDragging && Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        Vector2 mousePos = Input.mousePosition;
        hoveredWireBase = GetWireBaseAtPosition(mousePos);

        if (hoveredWireBase != null)
        {
            WirePair pair = GetWirePairForBase(hoveredWireBase);
            GlitchDot glitch = GetGlitchForBase(hoveredWireBase);

            if (pair != null && !pair.isConnected)
            {
                StartDragging(hoveredWireBase, pair.wireColor);
            }
            else if (pair != null && pair.isConnected)
            {
                // Remove connected wire
                RemoveWire(pair);
            }
            else if (glitch != null)
            {
                // Start dragging from glitch
                StartDragging(hoveredWireBase, glitch.glitchColor);
            }
        }
    }

    private void HandleMouseDrag()
    {
        if (currentlyDragging == null) return;

        Vector2 mousePos = Input.mousePosition;
        WirePath hoveredPath = GetWirePathAtPosition(mousePos);

        if (hoveredPath != null)
        {
            Vector2Int pathPos = GetPathPosition(hoveredPath);
            AddToPath(pathPos);
        }
    }

    private void HandleMouseUp()
    {
        if (currentlyDragging == null) return;

        Vector2 mousePos = Input.mousePosition;
        WireBase targetBase = GetWireBaseAtPosition(mousePos);

        if (targetBase != null)
        {
            TryConnectWire(targetBase);
        }
        else
        {
            // If we didn't connect to a base, check if we're connecting glitch to glitch
            CheckGlitchToGlitchConnection();
        }

        StopDragging();
    }

    private void StartDragging(WireBase startBase, Color wireColor)
    {
        currentlyDragging = startBase;
        isDragging = true;
        currentPath.Clear();

        // Clear any existing segments
        foreach (var segment in currentWireSegments)
        {
            if (segment != null) Destroy(segment.gameObject);
        }
        currentWireSegments.Clear();

        // Add starting position
        Vector2Int startPos = GetBasePosition(startBase);
        if (!IsPositionInGrid(startPos))
        {
            Debug.LogError($"Start position {startPos} is outside grid bounds!");
            return;
        }

        if (wirePaths == null)
        {
            Debug.LogError("wirePaths array is null!");
            return;
        }

        if (wirePaths[startPos.x, startPos.y] == null)
        {
            Debug.LogError($"WirePath at {startPos} is null!");
            return;
        }

        currentPath.Add(startPos);

        // Mark starting path if it's a wire path (not base)
        if (IsPositionInGrid(startPos))
        {
            wirePaths[startPos.x, startPos.y].SetTemporaryWire(wireColor);
        }
    }

    private void StopDragging()
    {
        // Remove incomplete wire
        foreach (var pos in currentPath)
        {
            if (IsPositionInGrid(pos))
            {
                wirePaths[pos.x, pos.y].ClearWire();
            }
        }

        foreach (var segment in currentWireSegments)
        {
            if (segment != null) Destroy(segment.gameObject);
        }
        currentWireSegments.Clear();
        currentPath.Clear();

        currentlyDragging = null;
        isDragging = false;
    }

    private void AddToPath(Vector2Int newPos)
    {
        if (currentPath.Count == 0) return;

        Vector2Int lastPos = currentPath[currentPath.Count - 1];

        // Check if new position is adjacent
        if (Mathf.Abs(newPos.x - lastPos.x) + Mathf.Abs(newPos.y - lastPos.y) != 1)
            return;

        // Check for backtracking/loops
        int existingIndex = currentPath.IndexOf(newPos);
        if (existingIndex != -1)
        {
            // Remove everything after the backtrack point
            for (int i = currentPath.Count - 1; i > existingIndex; i--)
            {
                RemoveLastSegment();
            }
            return;
        }


        // Check if position has a different glitch and not the starting glitch
        if (HasDifferentGlitchAtPosition(newPos, currentlyDragging))
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
            return;
        }

        // Block movinf over a non-matching wire base
        WireBase baseAtNewPos = GetWireBaseAtGridPosition(newPos);
        if (baseAtNewPos != null && baseAtNewPos != currentlyDragging)
        {
            WirePair currentPair = GetWirePairForBase(currentlyDragging);
            GlitchDot currentGlitch = GetGlitchForBase(currentlyDragging);
            WirePair targetPair = GetWirePairForBase(baseAtNewPos);
            GlitchDot targetGlitch = GetGlitchForBase(baseAtNewPos);
            
            bool sameWireColor = 
                (currentPair != null && targetPair != null && currentPair == targetPair) ||
                (currentGlitch != null && targetGlitch != null && currentGlitch == targetGlitch);

            if (!sameWireColor)
            {
                return; // Block movement over non-matching base
            }
        }

        // Check if path is occupied by another wire
        WirePath path = wirePaths[newPos.x, newPos.y];
        Color currentColor = GetCurrentWireColor();

        if (path.HasWire && path.WireColor != currentColor)
        {
            // Check if this is a glitch-wirebase collision
            WirePair currentPair = GetWirePairForBase(currentlyDragging);
            GlitchDot currentGlitch = GetGlitchForBase(currentlyDragging);

            // Find which wire pair owns the existing wire
            WirePair existingPair = GetWirePairByColor(path.WireColor);

            // Fail if current wire is from glitch and existing is from wirebase, or vice versa
            if ((currentGlitch != null && existingPair != null) ||
                (currentPair != null && existingPair == null))
            {
                PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
                return;
            }

            // Remove the existing wire (normal wire-to-wire collision)
            RemoveWireAtPath(newPos);
        }

        // Add new segment
        currentPath.Add(newPos);
        CreateWireSegment(lastPos, newPos, currentColor);

        // Mark path as occupied by current wire
        path.SetTemporaryWire(currentColor);
    }

    private bool HasDifferentGlitchAtPosition(Vector2Int gridPos, WireBase currentBase)
    {
        GlitchDot currentGlitch = GetGlitchForBase(currentBase);

        foreach (var glitch in glitchDots)
        {
            if (glitch.gridPosition == gridPos && glitch.glitchBase != currentBase)
            {
                return true; // Different glitch at this position
            }
        }
        return false;
    }

    private void RemoveLastSegment()
    {
        if (currentWireSegments.Count > 0)
        {
            WireSegment segment = currentWireSegments[currentWireSegments.Count - 1];
            currentWireSegments.RemoveAt(currentWireSegments.Count - 1);
            if (segment != null) Destroy(segment.gameObject);
        }

        if (currentPath.Count > 1)
        {
            Vector2Int removedPos = currentPath[currentPath.Count - 1];
            if (IsPositionInGrid(removedPos))
            {
                wirePaths[removedPos.x, removedPos.y].ClearWire();
            }
            currentPath.RemoveAt(currentPath.Count - 1);
        }
    }

    private void CreateWireSegment(Vector2Int from, Vector2Int to, Color color)
    {
        GameObject segmentObj = Instantiate(wireSegmentPrefab, gridContainer);
        WireSegment segment = segmentObj.GetComponent<WireSegment>();

        Vector2 fromWorld = GetWorldPosition(from);
        Vector2 toWorld = GetWorldPosition(to);

        float wireThickness = Mathf.Max(6f, cellSize * 0.1f);
        segment.Initialize(fromWorld, toWorld, color);
        currentWireSegments.Add(segment);
    }

    private void TryConnectWire(WireBase targetBase)
    {
        WirePair currentPair = GetWirePairForBase(currentlyDragging);
        WirePair targetPair = GetWirePairForBase(targetBase);
        GlitchDot currentGlitch = GetGlitchForBase(currentlyDragging);
        GlitchDot targetGlitch = GetGlitchForBase(targetBase);

        Color currentColor = GetCurrentWireColor();

        // Check for invalid connections
        if ((currentPair != null && targetGlitch != null) ||  // Wire base to glitch
        (currentGlitch != null && targetPair != null))    // Glitch to wire base
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
            return;
        }

        // Check if this is a valid wire pair connection
        if (currentPair != null && targetPair != null && currentPair == targetPair)
        {
            CompleteWire(currentPair, currentColor);
            CheckPuzzleCompletion();
        }
        // Check if connecting to different wirebase - BLOCK
        else if (currentPair != null && targetPair != null && currentPair != targetPair)
        {
            // Just stop dragging - don't complete the wire
            StopDragging();
        }
        // Check if this is a glitch connecting to itself (shouldn't normally happen)
        else if (currentGlitch != null && targetGlitch != null && currentGlitch == targetGlitch)
        {
            // Glitch connected to itself - just clear
            StopDragging();
        }
        else
        {
            // Invalid connection - just block
            StopDragging();
        }
    }

    private void CheckGlitchToGlitchConnection()
    {
        GlitchDot currentGlitch = GetGlitchForBase(currentlyDragging);

        // If we started from a glitch and didn't connect to any base, check if we connected to another glitch position
        if (currentGlitch != null && currentPath.Count > 1)
        {
            Vector2Int lastPos = currentPath[currentPath.Count - 1];

            // Check if the last position has another glitch
            foreach (var glitch in glitchDots)
            {
                if (glitch != currentGlitch && glitch.gridPosition == lastPos)
                {
                    // Connected two different glitches - failure
                    PuzzleManager.Instance.EndPuzzle(PuzzleResult.Failed);
                    return;
                }
            }
        }
    }

    private void CompleteWire(WirePair pair, Color color)
    {
        pair.isConnected = true;

        // Convert temporary segments to permanent
        foreach (var pos in currentPath)
        {
            if (!IsPositionInGrid(pos))
            {
                Debug.LogError($"Position {pos} in currentPath is outside grid bounds!");
                continue;
            }

            if (wirePaths[pos.x, pos.y] == null)
            {
                Debug.LogError($"WirePath at {pos} is null in CompleteWire!");
                continue;
            }

            wirePaths[pos.x, pos.y].SetPermanentWire(color);
        }

        currentWireSegments.Clear();
        currentPath.Clear();
    }

    private void RemoveWire(WirePair pair)
    {
        pair.isConnected = false;

        // Clear all paths occupied by this wire
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (wirePaths[x, y].WireColor == pair.wireColor)
                {
                    wirePaths[x, y].ClearWire();
                }
            }
        }

        // Remove all wire segments of this color
        foreach (Transform child in gridContainer)
        {
            WireSegment segment = child.GetComponent<WireSegment>();
            if (segment != null && segment.GetComponent<Image>().color == pair.wireColor)
            {
                Destroy(child.gameObject);
            }
        }

        // Clear from current segments list in case it's being dragged
        currentWireSegments.RemoveAll(segment => segment != null && segment.GetComponent<Image>().color == pair.wireColor);
    }

    private void RemoveWireAtPath(Vector2Int pathPos)
    {
        Color wireColor = wirePaths[pathPos.x, pathPos.y].WireColor;

        // Find and remove the wire pair that uses this color
        foreach (var pair in wirePairs)
        {
            if (pair.wireColor == wireColor && pair.isConnected)
            {
                RemoveWire(pair);
                break;
            }
        }
    }

    private void CheckPuzzleCompletion()
    {
        foreach (var pair in wirePairs)
        {
            if (!pair.isConnected)
                return;
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (HasGlitchAtPosition(new Vector2Int(x, y)))
                    continue;

                if (!wirePaths[x, y].HasWire)
                    return;
            }
        }

        // Mark as completed
        isCompleted = true;
        uiRoot?.SetActive(false); // Hide UI when completed
        active = false;

        PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
    }

    private bool HasGlitchAtPosition(Vector2Int gridPos)
    {
        foreach (var glitch in glitchDots)
        {
            if (glitch.gridPosition == gridPos)
                return true;
        }
        return false;
    }

    private Color GetCurrentWireColor()
    {
        WirePair pair = GetWirePairForBase(currentlyDragging);
        if (pair != null) return pair.wireColor;

        GlitchDot glitch = GetGlitchForBase(currentlyDragging);
        if (glitch != null) return glitch.glitchColor;

        return Color.white;
    }

    // Utility methods
    private WireBase GetWireBaseAtPosition(Vector2 screenPos)
    {
        foreach (var pair in wirePairs)
        {
            if (IsPositionOverUIElement(pair.startBase.GetComponent<RectTransform>(), screenPos))
                return pair.startBase;
            if (IsPositionOverUIElement(pair.endBase.GetComponent<RectTransform>(), screenPos))
                return pair.endBase;
        }

        foreach (var glitch in glitchDots)
        {
            if (IsPositionOverUIElement(glitch.glitchBase.GetComponent<RectTransform>(), screenPos))
                return glitch.glitchBase;
        }

        return null;
    }

    private WirePath GetWirePathAtPosition(Vector2 screenPos)
    {
        if (wirePaths == null)
        {
            Debug.LogError("wirePaths array is null in GetWirePathAtPosition!");
            return null;
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (wirePaths[x, y] == null)
                {
                    Debug.LogError($"WirePath at [{x},{y}] is null!");
                    continue;
                }

                RectTransform rect = wirePaths[x, y].GetComponent<RectTransform>();
                if (rect == null)
                {
                    Debug.LogError($"WirePath at [{x},{y}] has no RectTransform!");
                    continue;
                }

                if (IsPositionOverUIElement(rect, screenPos))
                    return wirePaths[x, y];
            }
        }

        return null;
    }

    private bool IsPositionOverUIElement(RectTransform rectTransform, Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPos);
    }

    private Vector2Int GetPathPosition(WirePath path)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (wirePaths[x, y] == path)
                    return new Vector2Int(x, y);
            }
        }
        return Vector2Int.zero;
    }

    private Vector2Int GetBasePosition(WireBase wireBase)
    {
        foreach (var pair in wirePairs)
        {
            if (pair.startBase == wireBase || pair.endBase == wireBase)
            {
                // Find which position this base is at
                if (pair.startBase == wireBase)
                    return GetGridPositionFromWorld(pair.startBase.GetComponent<RectTransform>().anchoredPosition);
                else
                    return GetGridPositionFromWorld(pair.endBase.GetComponent<RectTransform>().anchoredPosition);
            }
        }

        foreach (var glitch in glitchDots)
        {
            if (glitch.glitchBase == wireBase)
                return glitch.gridPosition;
        }

        return Vector2Int.zero;
    }

    private Vector2Int GetGridPositionFromWorld(Vector2 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x / cellSize) + (gridSize - 1) * 0.5f);
        int y = Mathf.RoundToInt((worldPos.y / cellSize) + (gridSize - 1) * 0.5f);
        return new Vector2Int(Mathf.Clamp(x, 0, gridSize - 1), Mathf.Clamp(y, 0, gridSize - 1));
    }

    private Vector2 GetWorldPosition(Vector2Int gridPos)
    {
        float x = (gridPos.x - (gridSize - 1) * 0.5f) * cellSize;
        float y = (gridPos.y - (gridSize - 1) * 0.5f) * cellSize;
        return new Vector2(x, y);
    }

    private WirePair GetWirePairForBase(WireBase wireBase)
    {
        foreach (var pair in wirePairs)
        {
            if (pair.startBase == wireBase || pair.endBase == wireBase)
                return pair;
        }
        return null;
    }

    private GlitchDot GetGlitchForBase(WireBase wireBase)
    {
        foreach (var glitch in glitchDots)
        {
            if (glitch.glitchBase == wireBase)
                return glitch;
        }
        return null;
    }

    private bool IsPositionInGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }

    // Initialization
    public override void StartPuzzle()
    {
        base.StartPuzzle();

        if (gridContainer == null)
        {
            Debug.LogError("Grid Container is not assigned in the inspector!");
            return;
        }

        // Ensure grid container has RectTransform
        RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
        if (gridRect == null)
        {
            Debug.LogError("Grid Container must have a RectTransform component!");
            return;
        }

        wirePairs.Clear();
        glitchDots.Clear();

        InitializeGrid();
        CreateWirePairsFromData();
        CreateGlitchDotsFromData();
        Debug.Log($"Grid initialized: {gridSize}x{gridSize}, wirePaths length: {wirePaths?.GetLength(0)}x{wirePaths?.GetLength(1)}");
    }

    private void InitializeGrid()
    {
        cellSize = gridWidth / gridSize;
        wirePaths = new WirePath[gridSize, gridSize];

        // Set up the grid container
        RectTransform gridRect = gridContainer.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(gridWidth, gridWidth);

        // Clear any existing children (useful for editor testing)
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Create wire paths
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject pathObj = Instantiate(wirePathPrefab, gridContainer);
                RectTransform pathRect = pathObj.GetComponent<RectTransform>();
                pathRect.sizeDelta = new Vector2(cellSize, cellSize);

                Vector2 position = GetWorldPosition(new Vector2Int(x, y));
                pathRect.anchoredPosition = position;

                wirePaths[x, y] = pathObj.GetComponent<WirePath>();
                wirePaths[x, y].Initialize(cellSize);
                pathObj.name = $"WirePath_{x}_{y}";
            }
        }
    }

    private void CreateWirePairsFromData()
    {
        foreach (var pairData in wirePairsData)
        {
            if (IsPositionValid(pairData.startPosition) && IsPositionValid(pairData.endPosition))
            {
                WirePair pair = new WirePair();
                pair.wireColor = pairData.wireColor;

                pair.startBase = CreateWireBase(pairData.startPosition, pairData.wireColor, false);
                pair.endBase = CreateWireBase(pairData.endPosition, pairData.wireColor, false);

                wirePairs.Add(pair);
            }
        }
    }

    private void CreateGlitchDotsFromData()
    {
        foreach (var glitchData in glitchDotsData)
        {
            if (IsPositionValid(glitchData.position))
            {
                GlitchDot glitch = new GlitchDot();
                glitch.glitchColor = glitchData.glitchColor;
                glitch.gridPosition = glitchData.position;
                glitch.glitchBase = CreateWireBase(glitchData.position, glitchData.glitchColor, true);

                glitchDots.Add(glitch);
            }
        }
    }

    private WireBase CreateWireBase(Vector2Int gridPos, Color color, bool isGlitch)
    {
        GameObject baseObj = Instantiate(wireBasePrefab, gridContainer);
        RectTransform baseRect = baseObj.GetComponent<RectTransform>();
        baseRect.sizeDelta = new Vector2(cellSize, cellSize);

        Vector2 position = GetWorldPosition(gridPos);
        baseRect.anchoredPosition = position;

        WireBase wireBase = baseObj.GetComponent<WireBase>();
        wireBase.Initialize(color, isGlitch, cellSize);

        return wireBase;
    }

    private bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridSize && position.y >= 0 && position.y < gridSize;
    }

    protected override void OnUIVisibilityChanged(bool visible)
    {
        // Show/hide the entire UI Canvas when puzzle starts/ends
        Transform uiCanvas = transform.Find("UICanvas");
        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(visible);
        }
    }

    private WirePair GetWirePairByColor(Color color)
    {
        foreach (var pair in wirePairs)
        {
            if (pair.wireColor == color)
                return pair;
        }
        return null;
    }

    private WireBase GetWireBaseAtGridPosition(Vector2Int gridPos)
    {
        foreach (var pair in wirePairs)
        {
            Vector2Int startPos = GetBasePosition(pair.startBase);
            Vector2Int endPos = GetBasePosition(pair.endBase);
            if (startPos == gridPos)
                return pair.startBase;
            if (endPos == gridPos)
                return pair.endBase;
        }
        foreach (var glitch in glitchDots)
        {
            if (glitch.gridPosition == gridPos)
                return glitch.glitchBase;
        }
        return null;
    }

    protected override void OnPuzzleReset()
    {
        // Clear all current puzzle elements
        if (gridContainer != null)
        {
            foreach (Transform child in gridContainer)
            {
                Destroy(child.gameObject);
            }
        }

        wirePairs.Clear();
        glitchDots.Clear();
        currentPath.Clear();
        currentWireSegments.Clear();
        currentlyDragging = null;
        hoveredWireBase = null;
        isDragging = false;

        // Reinitialize everything just like StartPuzzle
        InitializeGrid();
        CreateWirePairsFromData();
        CreateGlitchDotsFromData();
    }

    public bool IsPuzzleCompleted()
    {
        return isCompleted;
    }
}