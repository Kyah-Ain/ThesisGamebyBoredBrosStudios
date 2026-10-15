using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class OffScreenIndicator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("INDICATOR EVENTS")]
    [Space]
    public UnityEvent onIndicatorOnScreen;
    [Space]
    public UnityEvent onIndicatorOffScreen;
    
    [Header("REFERENCES")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform target;

    [Header("SETTINGS")]
    [SerializeField] private float edgePadding = 50f;
    [SerializeField] private bool hideWhenVisible = true;
    [SerializeField] private bool rotateArrow = true;
    
    [Header("OCCLUSION SETTINGS")]
    [SerializeField] private bool checkOcclusion = true;
    [SerializeField] private LayerMask occluderMask; // Layers that can hide the target (Buildings, Walls, Environment...)
    [SerializeField][ReadOnly] Vector3 targetOffset = Vector3.up; // Aim the check at the target's body instead of its feet/pivot

    [Header("STATUS")]
    [SerializeField][ReadOnly] bool _isInsideFrame;
    [SerializeField][ReadOnly] bool _isOccluded;
    [SerializeField][ReadOnly] bool _isVisibleToPlayer;
    
    private RectTransform _rectTransform;

    // Remembers the last state, so the events only fire when the state changes
    private bool _hasState;
    private bool _lastVisibleState;

    // ------------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        // Retrieve's the RectTransform component of the gameObject this scrip is attached to
        _rectTransform = this.GetComponent<RectTransform>();

        // Falls back to the main camera if none was assigned
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Stops if there is nothing to point at
        if (target == null || targetCamera == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Applies the offset and calculates the equivalent 2D coordinate of a 3D position
        Vector3 worldPos = target.position + targetOffset;
        Vector3 screenPos = targetCamera.WorldToScreenPoint(worldPos);

        // Checks both the camera frame and the 3D line of sight
        _isInsideFrame = IsInsideFrame(screenPos);
        _isOccluded = checkOcclusion && _isInsideFrame && IsOccluded(worldPos);
        _isVisibleToPlayer = _isInsideFrame && !_isOccluded;

        BroadcastState(_isVisibleToPlayer);
        
        // Nothing to show when the player can already see the target
        if (hideWhenVisible && _isVisibleToPlayer) return;
        
        // Moves the indicator and points the arrow
        Vector2 indicatorPos = GetIndicatorPosition(screenPos);
        _rectTransform.position = indicatorPos;
        
        // Rotates the indicator if the setting for it was enabled
        if (rotateArrow)
            RotateArrow(indicatorPos);
    }
    
    #endregion
    
    // ----------------------- CUSTOM METHODS -------------------------
    #region CUSTOM METHODS
    
    // Checks if the screen position is within the camera frame (and in front of the camera)
    private bool IsInsideFrame(Vector3 screenPos)
    {
        return screenPos.z > 0 &&
               screenPos.x >= 0 && screenPos.x <= Screen.width &&
               screenPos.y >= 0 && screenPos.y <= Screen.height;
    }

    // Checks if something is blocking the line between the camera and the target
    private bool IsOccluded(Vector3 worldPos)
    {
        // Linecast returns true when it hits something from the occluder layers
        // NOTE: Triggers are ignored so invisible trigger zones don't hide the target
        if (Physics.Linecast(targetCamera.transform.position, worldPos,
                out RaycastHit hit, occluderMask, QueryTriggerInteraction.Ignore))
        {
            // If we only hit the target itself (or its children), it's not really blocked
            return !hit.transform.IsChildOf(target);
        }

        return false;
    }

    // Fires the On/Off screen events only when the state actually changed
    private void BroadcastState(bool isVisible)
    {
        if (_hasState && _lastVisibleState == isVisible) return;

        _hasState = true;
        _lastVisibleState = isVisible;

        if (isVisible) onIndicatorOnScreen?.Invoke();
        else onIndicatorOffScreen?.Invoke();
    }

    // Gets where the indicator should sit, pushed to the screen edge when the target is outside the frame
    private Vector2 GetIndicatorPosition(Vector3 screenPos)
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (Vector2)screenPos - center;

        // Behind the camera, the projection is mirrored, so flip it back
        bool isBehind = screenPos.z < 0;
        if (isBehind) offset = -offset;

        // Largest offset allowed from the center while keeping the padding
        float halfWidth = Screen.width / 2f - edgePadding;
        float halfHeight = Screen.height / 2f - edgePadding;

        // How much the offset must be scaled to touch the padded screen edge
        float scaleX = halfWidth / Mathf.Max(Mathf.Abs(offset.x), 0.0001f);
        float scaleY = halfHeight / Mathf.Max(Mathf.Abs(offset.y), 0.0001f);
        float scale = Mathf.Min(scaleX, scaleY);

        // Pull in when outside the padded area, and always push to the edge when behind the camera
        if (scale < 1f || isBehind)
            offset *= scale;

        return center + offset;
    }

    // Rotates the arrow so it points away from the screen center
    private void RotateArrow(Vector2 indicatorPos)
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 direction = (indicatorPos - center).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    
    #endregion
}