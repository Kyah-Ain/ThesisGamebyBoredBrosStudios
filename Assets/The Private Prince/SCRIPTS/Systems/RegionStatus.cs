using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class RegionStatus : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public TextMeshProUGUI regionLiveStatus; // Text to tell what region the player currently is

    // -------------------------- METHODS -------------------------

    // Built-In Unity method that automatically called 1st 
    private void Awake()
    {
        UpdateStatus();
    }

    // Built-In Unity method that automatically called 2nd (when Active) 
    private void OnEnable()
    {
        // Subscribes 
        SaveManager.Instance.onEnteringNewRegion += UpdateStatus;
    }

    // Built-In Unity method that automatically called 2nd (when Inactive) 
    private void OnDisable()
    {
        // Unsubscribes 
        SaveManager.Instance.onEnteringNewRegion -= UpdateStatus;
    }

    // Built-In Unity method that automatically called 3rd (when the object was being destroyed)
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