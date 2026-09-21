using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.Events;

public class DialogueEffect : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("EFFECT EVENTS")] 
    public UnityEvent onEffectDone;

    [field: Header("SETTINGS")] 
    [field: SerializeField] public bool EnableNarrationEffect { get; private set; } = true; // Toggles if the Dialogue should have Narration effect or not
    [SerializeField] TextMeshProUGUI outputField; // Reference to the TextMesh that gonna display the effect
    [SerializeField, Range(0.01f, 100f)] float effectSpeed = 1f; // Speed for characters displayed per second
    
    [field: Header("STATUS")]
    public bool IsNarrating { get; private set; } // ...
    private Coroutine _currentEffectRunning; // Placeholder for the current narration effect running
    private string _currentDialogueToIterate; // Placeholder for the current character/s being applied with the effect
    
    // ------------------------ ACCESSIBLE METHODS ------------------------

    // Method to start a Narration Effect
    public void StartDialogueEffect(string dialogue)
    {
        // Tracks the current dialogue passed unto this script
        _currentDialogueToIterate = dialogue;
        
        // Checks if Narration Effect was toggled off
        if (!EnableNarrationEffect)
        {
            // Immediately outputs the dialogue without the effect
            SkipDialogue();

            // Skips executing further logics below
            return;
        }
        
        // Ensures that only one coroutine effect runs at a time
        if (_currentEffectRunning != null)
        {
            return;
        }
        
        // Starts and store the typewriting effect of the dialogue
        _currentEffectRunning = StartCoroutine(TypeWriterEffect(_currentDialogueToIterate));
    }

    // Method to Skip the narration
    public void SkipDialogue()
    {
        // Stops the current effect if one is currently running
        if (_currentEffectRunning != null)
        {
            StopCoroutine(_currentEffectRunning);
        }
        
        // Makes the coroutine placeholder available again
        _currentEffectRunning = null;

        // Overwrites the whole dialogue immediately, skipping the TypeWriting effect
        outputField.text = _currentDialogueToIterate;

        // Updates the effect status
        IsNarrating = false;

        // Broadcasts that the effect has finished
        onEffectDone?.Invoke();
    }

    // ---------------------------- EFFECTS -------------------------

    // Coroutine Method to iterate each letter in a sentence in an interval
    IEnumerator TypeWriterEffect(string dialogue)
    {
        // Updates the effect status
        IsNarrating = true;

        // Clears the previous dialogue before starting a new one
        outputField.text = "";

        // Iterates through each letter in a set of word/s or sentence/s
        foreach (char letter in dialogue)
        {
            // Displays each character individually
            outputField.text += letter;

            // Pauses the iteration before displaying the next character
            yield return new WaitForSeconds(1f / effectSpeed);
        }

        // Coroutine has naturally finished, so there's no need to StopCoroutine()
        ResetNarrationEffect();

        // Broadcasts that the effect has finished
        onEffectDone?.Invoke();
    }
    
    // Method to reset the Narration Effect
    void ResetNarrationEffect()
    {
        /*
            NOTE:
            This does NOT stop the coroutine.

            This method is used when the coroutine
            has already naturally reached its end.
        */

        // Effect is no longer narrating
        IsNarrating = false;

        // Makes the placeholder available for a new coroutine effect
        _currentEffectRunning = null;
    }
}
