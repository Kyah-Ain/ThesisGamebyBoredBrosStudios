using UnityEngine;
using UnityEngine.UI;

public class AmogusPuzzleGenerator : MonoBehaviour
{
    [Header("Puzzle")]
    public AmogusPuzzle puzzle;

    [Header("Generation")]
    public int pairCount = 4;

    public AmogusWireNode leftWirePrefab;
    public AmogusWireNode rightWirePrefab;

    public Transform leftParent;
    public Transform rightParent;

    [Header("Colors")]
    public Color[] wireColors =
    {
        Color.red,
        Color.blue,
        Color.yellow,
        Color.green,
        Color.magenta,
        Color.cyan,
        Color.white,
        new Color(1f,0.5f,0f)
    };

    private void Awake()
    {
        GeneratePuzzle();
    }

    [ContextMenu("Generate Puzzle")]
    public void GeneratePuzzle()
    {
        ClearChildren(leftParent);
        ClearChildren(rightParent);

        puzzle.leftNodes.Clear();
        puzzle.rightNodes.Clear();

        for (int i = 0; i < pairCount; i++)
        {
            AmogusWireNode left =
                Instantiate(leftWirePrefab);

            left.transform.SetParent(leftParent, false);

            RectTransform leftRect =
                left.GetComponent<RectTransform>();

            leftRect.localScale = Vector3.one;
            leftRect.localRotation = Quaternion.identity;
            leftRect.anchoredPosition = Vector2.zero;

            left.Initialize(
                i,
                wireColors[i % wireColors.Length],
                true,
                puzzle);

            puzzle.leftNodes.Add(left);

            //-------------------------------------------------

            AmogusWireNode right =
                Instantiate(rightWirePrefab);

            right.transform.SetParent(rightParent, false);

            RectTransform rightRect =
                right.GetComponent<RectTransform>();

            rightRect.localScale = Vector3.one;
            rightRect.localRotation = Quaternion.identity;
            rightRect.anchoredPosition = Vector2.zero;

            right.Initialize(
                i,
                wireColors[i % wireColors.Length],
                false,
                puzzle);

            puzzle.rightNodes.Add(right);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            leftParent as RectTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rightParent as RectTransform);
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(parent.GetChild(i).gameObject);
#else
            Destroy(parent.GetChild(i).gameObject);
#endif
        }
    }
}