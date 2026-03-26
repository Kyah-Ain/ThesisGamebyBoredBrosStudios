using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinueFlag : MonoBehaviour
{
    // --------------------------- VARIABLES -------------------------

    public ActivationManager activator;
    public Image buttonImage;

    // ------------------------- UNITY METHODS -------------------------

    private void OnEnable()
    {
        SaveManager.OnSaveStateChanged += Refresh;
        Refresh(); // sync state immediately on enable
    }

    private void OnDisable()
    {
        SaveManager.OnSaveStateChanged -= Refresh;
    }

    private void Refresh()
    {
        bool hasSave = SaveManager.Instance.HasSavedProgress;

        if (hasSave)
        {
            activator.BTNActivate();
            ColorUtility.TryParseHtmlString("#938989", out Color newColor);
            buttonImage.color = newColor;
        }
        else
        {
            activator.BTNDisable();
            buttonImage.color = Color.white;
        }
    }
}