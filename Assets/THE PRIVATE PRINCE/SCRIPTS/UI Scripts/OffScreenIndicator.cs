using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class OffScreenIndicator : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private float edgePadding = 50f;
    [SerializeField] private bool hideWhenVisible = true;
    [SerializeField] private bool rotateArrow = true;

    private RectTransform rectTransform;

    // ------------------------- UNITY METHODS -------------------------

    private void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (target == null || targetCamera == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 screenPos = targetCamera.WorldToScreenPoint(target.position);

        bool behindCamera = screenPos.z < 0;

        if (behindCamera)
        {
            screenPos *= -1;
        }

        bool onScreen =
            !behindCamera &&
            screenPos.x >= 0 &&
            screenPos.x <= Screen.width &&
            screenPos.y >= 0 &&
            screenPos.y <= Screen.height;

        if (hideWhenVisible && onScreen)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        screenPos.x = Mathf.Clamp(
            screenPos.x,
            edgePadding,
            Screen.width - edgePadding);

        screenPos.y = Mathf.Clamp(
            screenPos.y,
            edgePadding,
            Screen.height - edgePadding);

        rectTransform.position = screenPos;

        if (rotateArrow)
        {
            Vector2 screenCenter = new Vector2(
                Screen.width / 2f,
                Screen.height / 2f);

            Vector2 direction =
                ((Vector2)screenPos - screenCenter).normalized;

            float angle =
                Mathf.Atan2(direction.y, direction.x) *
                Mathf.Rad2Deg;

            rectTransform.rotation =
                Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}