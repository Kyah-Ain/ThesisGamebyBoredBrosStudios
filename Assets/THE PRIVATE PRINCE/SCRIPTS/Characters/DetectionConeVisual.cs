using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DetectionConeVisual : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] AIDetection detection;

    [Header("TOGGLES")]
    [SerializeField] bool showCone = true;        // The view cone (viewAngle / viewDistance)
    [SerializeField] bool showProximity = true;   // The 360° circle (proximityRadius)

    [Header("MESH SETTINGS")]
    [SerializeField, Range(10, 180)] int coneSegments = 60;
    [SerializeField, Range(10, 180)] int circleSegments = 60;
    [SerializeField] float heightOffset = 0.05f;
    [SerializeField] float wallOverlap = 0.05f;   // Pushes edges slightly into walls so there's no gap

    [Header("STYLES (drag ConeStyle assets here)")]
    [SerializeField] ConeStyle idleStyle;
    [SerializeField] ConeStyle alertStyle;
    [SerializeField, ReadOnly] ConeStyle currentStyle;

    Mesh mesh;
    MeshRenderer meshRenderer;
    MaterialPropertyBlock block;
    Vector3[] vertices;
    Vector2[] uvs;
    readonly List<int> triangles = new();

    int coneVertCount, circleVertCount;
    bool lastConeOn, lastCircleOn, trianglesDirty = true;

    void Awake()
    {
        if (detection == null) detection = GetComponentInParent<AIDetection>();

        mesh = new Mesh { name = "DetectionVisual" };
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();

        BuildArrays();
        SetStyle(idleStyle);
    }

    // ---------- STYLE SWAPPING ----------
    public void SetAlertOn()  => SetStyle(alertStyle);
    public void SetAlertOff() => SetStyle(idleStyle);

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
    // Layout: [cone strip: inner/outer pairs][circle fan]
    int ConeVerts => (coneSegments + 1) * 2;
    int CircleVerts => circleSegments + 2;

    void BuildArrays()
    {
        coneVertCount = ConeVerts;
        circleVertCount = CircleVerts;

        vertices = new Vector3[coneVertCount + circleVertCount];
        uvs = new Vector2[vertices.Length];

        // Cone strip UVs: v = 0 at the inner edge, 1 at the outer edge
        for (int i = 0; i <= coneSegments; i++)
        {
            float u = i / (float)coneSegments;
            uvs[i * 2]     = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);
        }

        // Circle fan UVs: v = 0 at the center, 1 at the edge
        int c = coneVertCount;
        uvs[c] = new Vector2(0.5f, 0f);
        for (int i = 0; i <= circleSegments; i++)
            uvs[c + i + 1] = new Vector2(i / (float)circleSegments, 1f);

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        trianglesDirty = true;
    }

    void BuildTriangles(bool coneOn, bool circleOn)
    {
        triangles.Clear();

        if (coneOn)
        {
            for (int i = 0; i < coneSegments; i++)
            {
                int a = i * 2; // inner i, outer i = a+1, inner i+1 = a+2, outer i+1 = a+3
                triangles.Add(a);     triangles.Add(a + 1); triangles.Add(a + 3);
                triangles.Add(a);     triangles.Add(a + 3); triangles.Add(a + 2);
            }
        }

        if (circleOn)
        {
            for (int i = 0; i < circleSegments; i++)
            {
                triangles.Add(coneVertCount);
                triangles.Add(coneVertCount + i + 1);
                triangles.Add(coneVertCount + i + 2);
            }
        }

        mesh.SetTriangles(triangles, 0);
    }

    void LateUpdate()
    {
        if (vertices.Length != ConeVerts + CircleVerts) BuildArrays();
        DrawShape();
    }

    void DrawShape()
    {
        Transform origin = detection.ViewOrigin;
        float coneAngle = Mathf.Clamp(detection.viewAngle, 0f, 360f);

        bool coneOn = showCone && detection.viewDistance > 0f && coneAngle > 0f;
        bool circleOn = showProximity && detection.proximityRadius > 0f;

        if (trianglesDirty || coneOn != lastConeOn || circleOn != lastCircleOn)
        {
            BuildTriangles(coneOn, circleOn);
            lastConeOn = coneOn;
            lastCircleOn = circleOn;
            trianglesDirty = false;
        }

        Vector3 center = origin.position + Vector3.up * heightOffset;

        if (coneOn)
        {
            float innerRadius = 0f;
            if (circleOn)
            {
                // Start inside the circle polygon's chords so there's never a gap (the overlap is a hairline)
                float inscribed = detection.proximityRadius * Mathf.Cos(Mathf.PI / circleSegments);
                innerRadius = Mathf.Min(inscribed, detection.viewDistance) * 0.97f;
            }
            FillCone(origin, coneAngle, innerRadius, center);
        }

        if (circleOn)
            FillCircle(origin, center);

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    void FillCone(Transform origin, float angle, float innerRadius, Vector3 center)
    {
        float step = angle / coneSegments;

        for (int i = 0; i <= coneSegments; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, -angle / 2f + step * i, 0f) * detection.ViewForward;

            float outer = detection.viewDistance;
            if (Physics.Raycast(origin.position, dir, out RaycastHit hit, outer, detection.obstacleMask))
            {
                outer = Mathf.Min(hit.distance + wallOverlap, outer);

                if (hit.distance < 0.5f)
                    Debug.Log($"Cone ray {i} hit '{hit.collider.name}' (layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}) at {hit.distance:F2}", hit.collider);
            }

            float inner = Mathf.Min(innerRadius, outer);

            Vector3 innerPoint = origin.position + dir * inner;
            Vector3 outerPoint = origin.position + dir * outer;
            innerPoint.y = outerPoint.y = center.y;

            vertices[i * 2]     = transform.InverseTransformPoint(innerPoint);
            vertices[i * 2 + 1] = transform.InverseTransformPoint(outerPoint);
        }
    }

    void FillCircle(Transform origin, Vector3 center)
    {
        int start = coneVertCount;
        vertices[start] = transform.InverseTransformPoint(center);
        float step = 360f / circleSegments;

        for (int i = 0; i <= circleSegments; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, step * i, 0f) * Vector3.forward;

            Vector3 endPoint = origin.position + dir * detection.proximityRadius;
            if (Physics.Raycast(origin.position, dir, out RaycastHit hit, detection.proximityRadius, detection.obstacleMask))
            {
                endPoint = hit.point + dir * wallOverlap;

                if (hit.distance < 0.5f)
                    Debug.Log($"Circle ray {i} hit '{hit.collider.name}' (layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}) at {hit.distance:F2}", hit.collider);
            }

            endPoint.y = center.y;
            vertices[start + i + 1] = transform.InverseTransformPoint(endPoint);
        }
    }
}