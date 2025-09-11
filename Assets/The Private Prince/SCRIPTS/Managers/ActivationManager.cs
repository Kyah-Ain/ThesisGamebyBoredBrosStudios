using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.UI; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.
using System.Reflection; // Grants access to reflection classes, allowing you to inspect and invoke methods dynamically

public class ActivationManager : MonoBehaviour
{
    // ---------------------------- VARIABLES -------------------------
    [Header("Button Attribute")]
    public Button[] buttonsToEnable; // Reference to the buttons that will be enabled
    public Button[] buttonsToDisable; // Reference to the buttons that will be disabled

    [Header("UI References")]
    public GameObject[] gameObjectsToActivate; // Reference to the game object that will be activated 
    public GameObject[] gameObjectsToDisable; // Reference to the currently active game object in the scene

    // ---------------------------- METHODS ---------------------------
    // Method to "Activate" a GameObject or UI element
    public virtual void Activate()
    {
        for (int objectToA = 0; objectToA < gameObjectsToActivate.Length; objectToA++)
        {
            gameObjectsToActivate[objectToA].SetActive(true); // Activates
        }
    }

    // Method to "Deactivate" a GameObject or UI element
    public virtual void Disable()
    {
        for (int objectToD = 0; objectToD < gameObjectsToDisable.Length; objectToD++) 
        {
            gameObjectsToDisable[objectToD].SetActive(false); // Deactivates
        }
    }

    // "Disables" current GameObject or UI/Panel and "Activates" new or existing one
    public virtual void Switch() 
    {
        if (gameObjectsToDisable != null || gameObjectsToActivate != null) 
        {
            Activate();
            Disable();
        }
        else
        {
            Debug.LogError("One of the reference in SwitchPanel is empty! If action is intended, use another method.");
        }
    }

    // Method to "Activate" a button interaction
    public virtual void BTNActivate()
    {
        for (int buttonsToA = 0; buttonsToA < buttonsToEnable.Length; buttonsToA++)
        {
            buttonsToEnable[buttonsToA].interactable = true; // Activates
        }
    }

    // Method to "Deactivate" a button interaction
    public virtual void BTNDisable()
    {
        for (int buttonsToD = 0; buttonsToD < buttonsToDisable.Length; buttonsToD++)
        {
            buttonsToDisable[buttonsToD].interactable = false; // Deactivates
        }
    }

    // "Disables" current button interaction and "Activates" new or existing one
    public virtual void BTNSwitch()
    {
        if (buttonsToDisable != null || buttonsToEnable != null) // Deactivates the current scene if assigned
        {
            BTNActivate();
            BTNDisable();
        }
        else
        {
            Debug.LogError("One of the reference in SwitchPanel is empty! If action is intended, use another method.");
        }
    }
}
