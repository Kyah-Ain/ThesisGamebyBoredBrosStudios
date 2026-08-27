using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AmogusPuzzle : PuzzleBase
{
    [SerializeField]
    private Transform wireContainer;

    [Header("Wire Nodes")]
    public List<AmogusWireNode> leftNodes = new List<AmogusWireNode>();
    public List<AmogusWireNode> rightNodes = new List<AmogusWireNode>();

    [Header("Wire Line")]
    public AmogusWireUI wirePrefab;
    public Transform WireContainer => wireContainer;

    private readonly List<AmogusWireConnection> connections = new();

    private AmogusWireNode currentStartNode;
    private AmogusWireUI currentLine;

    public AmogusWireNode CurrentHoverNode { get; set; }

    private int completedPairs;

    private void Awake()
    {

    }

    public override void StartPuzzle()
    {
        base.StartPuzzle();

        completedPairs = 0;

        ShuffleRightSide();

        foreach (var left in leftNodes)
            left.ResetNode();

        foreach (var right in rightNodes)
            right.ResetNode();

        foreach (var connection in connections)
        {
            if (connection.line != null)
                Destroy(connection.line.gameObject);
        }

        connections.Clear();

        ShuffleRightSide();
    }

    protected override void OnPuzzleReset()
    {
        completedPairs = 0;

        foreach (var left in leftNodes)
            left.ResetNode();

        foreach (var right in rightNodes)
            right.ResetNode();

        foreach (var connection in connections)
        {
            if (connection.line != null)
                Destroy(connection.line.gameObject);
        }

        connections.Clear();

        ShuffleRightSide();
    }

    public override void HandleInput()
    {
        if (currentLine == null)
            return;

        Vector2 mouse;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            wireContainer as RectTransform,
            Input.mousePosition,
            null,
            out mouse);

        Debug.Log($"Mouse Position: {mouse}");

        currentLine.UpdateEnd(mouse);

        if (Input.GetMouseButtonUp(0))
        {
            FinishConnection();
        }
    }

    public void BeginConnection(AmogusWireNode startNode)
    {
        if (currentLine != null)
            return;

        currentStartNode = startNode;

        currentLine =
            Instantiate(
                wirePrefab,
                wireContainer);

        currentLine.Begin(
            startNode.WirePosition,
            startNode.WireColor);
    }

    private void FinishConnection()
    {
        if (currentStartNode == null)
        {
            CancelCurrentLine();
            return;
        }

        if (CurrentHoverNode == null)
        {
            CancelCurrentLine();
            return;
        }

        if (CurrentHoverNode.connected)
        {
            CancelCurrentLine();
            return;
        }

        if (CurrentHoverNode.isLeftNode)
        {
            CancelCurrentLine();
            return;
        }

        if (!currentStartNode.Matches(CurrentHoverNode))
        {
            CancelCurrentLine();
            return;
        }

        currentLine.Finish(CurrentHoverNode.WirePosition);

        currentStartNode.Connect();
        CurrentHoverNode.Connect();

        connections.Add(new AmogusWireConnection(
            currentStartNode,
            CurrentHoverNode,
            currentLine));

        completedPairs++;

        int totalPairs = Mathf.Min(leftNodes.Count, rightNodes.Count);

        if (completedPairs >= totalPairs)
        {
            PuzzleManager.Instance.EndPuzzle(PuzzleResult.Solved);
        }

        currentStartNode = null;
        currentLine = null;
        CurrentHoverNode = null;
    }

    private void CancelCurrentLine()
    {
        if (currentLine != null)
            currentLine.Cancel();

        currentLine = null;
        currentStartNode = null;
        CurrentHoverNode = null;
    }

    private void ShuffleRightSide()
    {
        List<Transform> positions = new();

        foreach (AmogusWireNode node in rightNodes)
        {
            positions.Add(node.transform);
        }

        for (int i = positions.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (positions[i], positions[random]) =
                (positions[random], positions[i]);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            positions[i].SetSiblingIndex(i);
        }
    }

    private void OnValidate()
    {
        if (leftNodes.Count != rightNodes.Count)
        {
            Debug.LogWarning(
                "Left and Right node counts do not match.",
                this);
        }

        if (wirePrefab == null)
        {
            Debug.LogWarning(
                "Wire Prefab not assigned.",
                this);
        }

        if (wireContainer == null)
        {
            Debug.LogWarning(
                "Wire Container not assigned.",
                this);
        }
    }
}