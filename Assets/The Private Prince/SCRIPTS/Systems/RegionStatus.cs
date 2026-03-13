using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class RegionStatus : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public TextMeshProUGUI regionLiveStatus; // Text to tell what region the player currently is

    // -------------------------- METHODS -------------------------

    // ...
    private void Awake()
    {
        UpdateStatus();
    }

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        // Subscribes 
        SaveManager.Instance.onEnteringNewRegion += UpdateStatus;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        // Unsubscribes 
        SaveManager.Instance.onEnteringNewRegion -= UpdateStatus;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        // Unsubscribes
        SaveManager.Instance.onEnteringNewRegion -= UpdateStatus;
    }

    // ...
    public void UpdateStatus() 
    {
        if (SaveManager.Instance.currentRegionPoint != null && 
            SaveManager.Instance.currentRegionPoint.Length > 0)
        {
            // ...
            regionLiveStatus.text = SaveManager.Instance.currentRegionPoint;
        }
    }
}