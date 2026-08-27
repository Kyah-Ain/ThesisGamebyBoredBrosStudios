using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AmogusWireLine : MonoBehaviour
{
    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.enabled = false;
    }

    public void Begin(Vector3 startPosition)
    {
        line.enabled = true;

        line.SetPosition(0, startPosition);
        line.SetPosition(1, startPosition);
    }

    public void UpdateEnd(Vector3 endPosition)
    {
        if (!line.enabled)
            return;

        line.SetPosition(1, endPosition);
    }

    public void Finish(Vector3 endPosition)
    {
        line.SetPosition(1, endPosition);
    }

    public void Cancel()
    {
        line.enabled = false;
    }
}