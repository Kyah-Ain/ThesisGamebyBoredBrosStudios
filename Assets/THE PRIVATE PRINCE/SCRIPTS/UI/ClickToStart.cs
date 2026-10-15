using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ClickToStart : MonoBehaviour
{
    // ------------------------- LOCAL ENUM -------------------------

    enum SkipType
    {
        SkipManually,
        SkipOnInteract
    }
    
    // ------------------------- VARIABLES -------------------------
    #region VARIABLES

    [SerializeField] UnityEvent onClick;
    
    [SerializeField] Animator animator;
    
    [SerializeField][ReadOnly] bool isInteracted;
    
    #endregion
    
    // ----------------------- UNITY METHODS ------------------------
    #region UNITY METHODS
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // OnEnable is called when the object becomes enabled and active
    protected virtual void OnEnable()
    {
        // Subscribes the method to an Input Event
        GameEventsManager.Instance.inputEvents.onSubmit += SkipAnimation;
    }
    
    // OnDisable is called when the object becomes disabled
    protected virtual void OnDisable()
    {
        // UnSubscribes the method to an Input Event
        GameEventsManager.Instance.inputEvents.onSubmit -= SkipAnimation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #endregion
    
    // ---------------------- USEABLE METHODS ------------------------
    #region USEABLE METHODS
    
    // Method for skipping the animation
    public void SkipAnimation()
    {
        animator.SetTrigger("isInteracted");
    }
    
    // Overload Method for skipping the animation
    void SkipAnimation(InputAction.CallbackContext context)
    {
        // Proceed only if skip hasn't been interacted yet
        if (!isInteracted)
        {
            // Flags that the interaction for this animation has already been used
            isInteracted = true;
            
            SkipAnimation();
            
            // Trigger the Event
            onClick?.Invoke();
        }
    }

    // Method for triggering the Interactable to an animation
    public void SetThisInteractable()
    {
        // Resets the flag for other animations to use
        isInteracted = false;
    }

    #endregion
}
