using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannableObjects : MonoBehaviour
{
    // -------------------------- VARIABLES -------------------------

    public bool onCyberScan = false; // ...

    Renderer objRenderer;

    public Material unScannedColor; // ...
    public Material scannedColor; // ...

    // ------------------------- UNITY METHODS -------------------------

    // ...
    private void OnEnable()
    {
        // Get the Renderer component and change its material
        objRenderer = this.GetComponent<Renderer>();
    }

    // ...
    private void OnDisable()
    {
        objRenderer = null;
    }

    // ...
    public void InCyberScan()
    {
        // Toggle the boolean
        onCyberScan = !onCyberScan;

        // ...
        if (objRenderer != null)
        {
            // ...
            objRenderer.material = onCyberScan ? scannedColor : unScannedColor;
        }
    }
}