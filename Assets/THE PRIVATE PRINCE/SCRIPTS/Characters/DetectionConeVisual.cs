using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DetectionConeVisual : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] AIDetection detection;

    [Header("MESH SETTINGS")]
    [SerializeField, Range(10, 120)] int segments = 40;
    [SerializeField] float heightOffset = 0.05f;

    [Header("STYLES (drag ConeStyle assets here)")]
    [SerializeField] ConeStyle idleStyle;
    [SerializeField] ConeStyle alertStyle;
    [SerializeField, ReadOnly] ConeStyle currentStyle;

    Mesh mesh;
    MeshRenderer meshRenderer;
    MaterialPropertyBlock block;
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    void Awake()
    {
        if (detection == null) detection = GetComponentInParent<AIDetection>();

        mesh = new Mesh { name = "DetectionCone" };
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();

        BuildArrays();
        SetStyle(idleStyle);
    }

    // ---------- STYLE SWAPPING (call from code, UnityEvents, or other scripts) ----------
    public void SetAlertOn()  => SetStyle(alertStyle);
    public void SetAlertOff() => SetStyle(idleStyle);

    // Swap to ANY style at runtime (e.g. a "Searching" or "Boss" style)
    public void SetStyle(ConeStyle style)
    {
        if (style == null || style == currentStyle) return;
        currentStyle = style;

        if (style.material != null) meshRenderer.sharedMaterial = style.material;

        meshRenderer.GetPropertyBlock(block);
        block.Clear();
        if (style.useTint && meshRenderer.sharedMaterial != null)
        {
            int id = Shader.PropertyToID(style.colorProperty);
            if (meshRenderer.sharedMaterial.HasProperty(id)) block.SetColor(id, style.tint);
        }
        meshRenderer.SetPropertyBlock(block);
    }

    // ---------- MESH ----------
    void BuildArrays()
    {
        vertices = new Vector3[segments + 2];
        uvs = new Vector2[segments + 2];
        triangles = new int[segments * 3];

        // UVs for custom shaders: v = 0 at the tip, 1 at the far edge; u = 0..1 left to right
        uvs[0] = new Vector2(0.5f, 0f);
        for (int i = 0; i <= segments; i++) uvs[i + 1] = new Vector2(i / (float)segments, 1f);

        for (int i = 0; i < segments; i++)
        {
            int t = i * 3;
            triangles[t]     = 0;
            triangles[t + 1] = i + 1;
            triangles[t + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    void LateUpdate()
    {
        if (vertices.Length != segments + 2) BuildArrays(); // in case segments is edited in play mode
        DrawCone();
    }

    void DrawCone()
    {
        Transform origin = detection.ViewOrigin;
        float angle = detection.viewAngle;
        float distance = detection.viewDistance;

        vertices[0] = transform.InverseTransformPoint(origin.position + Vector3.up * heightOffset);

        float startAngle = -angle / 2f;
        float step = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, startAngle + step * i, 0f) * origin.forward;

            Vector3 endPoint = origin.position + dir * distance;
            if (Physics.Raycast(origin.position, dir, out RaycastHit hit, distance, detection.obstacleMask))
                endPoint = hit.point;

            endPoint.y = origin.position.y + heightOffset;
            vertices[i + 1] = transform.InverseTransformPoint(endPoint);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }
}