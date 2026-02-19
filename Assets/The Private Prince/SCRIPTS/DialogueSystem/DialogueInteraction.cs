using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteraction : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("DIALOGUE")]
    [SerializeField] protected DialogueUI dialogueUI;

    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    public int dialogueIterationLimit;

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public virtual void Awake()
    {
        Debug.Log("DialogueInteraction Awake called, attempting to find DialogueUI component...");
    }

    // ...
    public virtual void Start()
    {
        Debug.Log("DialogueInteraction Start called, DialogueUI reference: " + (dialogueUI != null ? "Found" : "Not Found"));
    }

    // ...
    public virtual void Update()
    {
        Interactable?.Interact(this); // Used null propagation for less lines
    }
}