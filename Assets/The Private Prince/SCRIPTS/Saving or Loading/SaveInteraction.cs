using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Events;

public class SaveInteraction : MonoBehaviour
{
    // ------------------------- EVENTS -------------------------

    public event Action onSave;

    public UnityEvent doorVFX;
    public UnityEvent doorSFX;

    // ------------------------- VARIABLES -------------------------

    PrivatePrinceControls ppControls; // Reference to the PrivatePrinceControls script that handles the new input system controls
    public GameObject savingCanvasUI; // Reference to the Canvas Prompt to activate when saving was Interacted

    [Header("TO SAVE VARIABLES")]

    public Transform spawnPointHolder; // Holds the location on where the spawnPoint should appear
    public float MUSIC; // Reference to the current Music loudness
    public float SFX; // Reference to thee current In-Game sounds volume

    public enum InteractionType // List of options for the Interaction Type
    {
        Open, // Instantly triggerable just by entering the trigger area
        Interactable // Requires the player to interact
    }

    // Sets the default door type to an open door (the most use cases)
    public InteractionType interactionType = InteractionType.Interactable;

    [SerializeField] private bool isInteractable = false; // Flag to track if the door has been interacted 

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        ClosePrompt();
        SubscribeToInputEvents();

        onSave += OpenPrompt;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        UnsubscribeFromInputEvents();

        onSave -= OpenPrompt;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        SubscribeToInputEvents();

        onSave -= OpenPrompt;
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        isInteractable = true;
    }

    // ...
    private void OnTriggerStay(Collider actor)
    {
        if (!actor.CompareTag("Player")) return;

        // Check if ppControls is null and try to get it again
        if (ppControls == null)
        {
            SubscribeToInputEvents();
        }

        // ...
        if (isInteractable &&
           (ppControls.Player.Interact.WasPerformedThisFrame() ||
            interactionType == InteractionType.Open))
        {
            onSave?.Invoke();

            doorVFX?.Invoke();
            doorSFX?.Invoke();
        }
    }

    // ...
    private void OnTriggerExit(Collider actor)
    {
        isInteractable = false;
    }

    // ------------------------- EVENT METHODS -------------------------

    // ...
    private void SubscribeToInputEvents()
    {
        // Get the reference to the PrivatePrinceControls script that handles the new input system controls
        ppControls = GameplayInputManager.Instance?.Controls;

        if (ppControls == null) return;

        // Unsubscribe first to prevent double or multiple subscriptions 
        UnsubscribeFromInputEvents();

        Debug.Log("ResponseHandler: Subscribed to input events");
    }

    // ...
    private void UnsubscribeFromInputEvents()
    {
        if (ppControls == null) return;

        Debug.Log("ResponseHandler: Unsbscribed to input events");
    }

    // ------------------------- CUSTOM METHODS -------------------------

    // Method that would prompt user to Save when they interact
    public void OpenPrompt() 
    {
        savingCanvasUI.SetActive(true);
    }

    // Method that would prompt user to Save when they interact
    public void ClosePrompt()
    {
        savingCanvasUI.SetActive(false);
    }

    // Method that executes what happens when you save a session In-Game (Best to call from a Unity Button)
    public void SaveSession()
    {
        // Sets the position of the spawnPoint to clamped on the specified position of spawnPointHolder
        SaveManager.Instance.spawnPoint.position = spawnPointHolder.transform.position;

        // Sets the current In-Game Sounds Volume that would be saved
        SaveManager.Instance.MUSIC = this.MUSIC;
        SaveManager.Instance.SFX = this.SFX;

        // EXECUTES THE SAVING (from the SaveManager.cs Script)
        SaveManager.Instance.Save();
    }
}