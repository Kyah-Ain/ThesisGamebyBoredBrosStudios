using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CameraOccluderFader : MonoBehaviour
{
    [SerializeField] private LayerMask occluderMask;
    [SerializeField] private Vector3 targetOffset = Vector3.zero;
    [SerializeField] private float padding = 0.3f;
    [SerializeField] private float targetClearance = 1f;
    [SerializeField] private Transform currentTarget;

    private CinemachineBrain brain;
    private readonly Dictionary<Transform, List<Renderer>> buildings = new Dictionary<Transform, List<Renderer>>();
    private readonly Dictionary<Renderer, ShadowCastingMode> hidden = new Dictionary<Renderer, ShadowCastingMode>();
    private readonly HashSet<Renderer> current = new HashSet<Renderer>();
    private readonly List<Renderer> toRestore = new List<Renderer>();

    private void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        Rebuild();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        foreach (KeyValuePair<Renderer, ShadowCastingMode> pair in hidden)
        {
            if (pair.Key != null) pair.Key.shadowCastingMode = pair.Value;
        }
        hidden.Clear();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Rebuild();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Rebuild();
    }

    private void Rebuild()
    {
        buildings.Clear();
        Renderer[] all = FindObjectsOfType<Renderer>();
        for (int i = 0; i < all.Length; i++)
        {
            Renderer r = all[i];
            if (!r.enabled) continue;
            int layer = r.gameObject.layer;
            if ((occluderMask.value & (1 << layer)) == 0) continue;
            Transform root = r.transform;
            while (root.parent != null && root.parent.gameObject.layer == layer) root = root.parent;
            if (!buildings.TryGetValue(root, out List<Renderer> parts))
            {
                parts = new List<Renderer>();
                buildings.Add(root, parts);
            }
            parts.Add(r);
        }
    }

    private void LateUpdate()
    {
        ICinemachineCamera live = brain != null ? brain.ActiveVirtualCamera : null;
        currentTarget = live != null ? (live.LookAt != null ? live.LookAt : live.Follow) : null;

        current.Clear();

        if (currentTarget != null)
        {
            Vector3 camPos = transform.position;
            Vector3 toTarget = currentTarget.position + targetOffset - camPos;
            float length = toTarget.magnitude - targetClearance;
            if (length > 0.01f)
            {
                Ray ray = new Ray(camPos, toTarget.normalized);
                foreach (KeyValuePair<Transform, List<Renderer>> pair in buildings)
                {
                    if (pair.Key == null) continue;
                    List<Renderer> parts = pair.Value;
                    for (int i = 0; i < parts.Count; i++)
                    {
                        Renderer r = parts[i];
                        if (r == null) continue;
                        Bounds b = r.bounds;
                        b.Expand(padding * 2f);
                        if (b.IntersectRay(ray, out float d) && d <= length)
                        {
                            for (int j = 0; j < parts.Count; j++)
                            {
                                if (parts[j] != null) current.Add(parts[j]);
                            }
                            break;
                        }
                    }
                }
            }
        }

        foreach (Renderer r in current)
        {
            if (hidden.ContainsKey(r)) continue;
            hidden[r] = r.shadowCastingMode;
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

        toRestore.Clear();
        foreach (KeyValuePair<Renderer, ShadowCastingMode> pair in hidden)
        {
            if (pair.Key == null || !current.Contains(pair.Key)) toRestore.Add(pair.Key);
        }

        for (int i = 0; i < toRestore.Count; i++)
        {
            Renderer r = toRestore[i];
            if (r != null) r.shadowCastingMode = hidden[r];
            hidden.Remove(r);
        }
    }
}