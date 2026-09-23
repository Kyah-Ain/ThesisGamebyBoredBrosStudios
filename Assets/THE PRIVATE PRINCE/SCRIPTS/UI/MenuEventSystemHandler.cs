using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events; 
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

// Requires a DebuggerKey.cs for it to debug, otherwise it will be muted
[RequireComponent(typeof(DebuggerNiAinPjls))]

public class MenuEventSystemHandler : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("SELECTION EVENTS")]
    public UnityEvent onSelect; // Event that fires in the Inspector

    [Header("REFERENCE")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    protected Tween _scaleUpTween; // Reference for the scaled-Up version of the selected UI
    protected Tween _scaleDownTween; // Reference for the scaled-Down version of the selected UI

    [Header("STATUS")]
    public List<Selectable> Selectables = new List<Selectable>(); // List of interactable UI objects to navigate
    [SerializeField] protected Selectable _firstSelected; // Initial interactable UI object to select
    protected Selectable _lastSelected; // Tracks the last UI interacted

    // Container for the original/default scales of each UI elements before any animation (for reset purposes)
    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    [Header("ANIMATION SETTINGS")]
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>(); // List of excluded interactable UI objects to animate
    [SerializeField] protected float _selectedAnimationScale = 1.1f; // The size of how big the selected interactable UI can be magnified
    [SerializeField] protected float _scaleDuration = 0.25f; // The speed of magnification's animation

    // ----------------------- UNITY METHODS -------------------------

    // Awake is called when the script instance is being loaded
    public virtual void Awake()
    {
        // Try to find optional DebuggerComponent on this GameObject
        debuggerNiAin = GetComponent<DebuggerNiAinPjls>();

        // Iterates through each UI inserted the list
        foreach (var selectable in Selectables) 
        {
            // Converts a simple UI to a navigable with keys type of UI 
            AddSelectionListeners(selectable);

            // Stores the UI's original/initial size before animation
            _scales.Add(selectable, selectable.transform.localScale);  
        }
    }

    // OnEnable is called when the object becomes enabled and active
    public virtual void OnEnable() 
    {
        Subscribe();

        // Iterates through each UI inserted the list
        for (int i = 0; i < Selectables.Count; i++)
        {
            // Resets the UI back to its original/initial size before animation
            Selectables[i].transform.localScale = _scales[Selectables[i]];
        }

        StartCoroutine(SelectAfterDelay());
    }

    // OnEnable is called when the object becomes disabled and inactive
    void OnDisable()
    {
        Unsubscribe();

        // Stops trying to animate the interactable UI's when they're disabled
        _scaleUpTween.Kill(true);
        _scaleDownTween.Kill(true);
    }

    // OnDestroy is called when the object is destroyed
    void OnDestroy()
    {
        Unsubscribe();

        // Stops trying to animate the interactable UI's when they're destroyed
        _scaleUpTween.Kill(true);
        _scaleDownTween.Kill(true);
    }

    // ----------------------- SUBSCRIPTION METHODS -------------------------

    // Method to subscribe to events as a listener
    public void Subscribe()
    {
        // Proceeds only if the Input Control manager was found
        if (GameEventsManager.Instance == null) return;

        // SUBSCRIBE METHODS to the input action events
        GameEventsManager.Instance.inputEvents.onNavigate += OnNavigate;
        GameEventsManager.Instance.inputEvents.onSubmit += OnSubmit;
    }

    // Method to unsubscribe from events 
    public void Unsubscribe()
    {
        // Proceeds only if the Input Control manager was found
        if (GameEventsManager.Instance == null) return;

        // UNSUBSCRIBE METHODS to the input action events
        GameEventsManager.Instance.inputEvents.onNavigate -= OnNavigate;
        GameEventsManager.Instance.inputEvents.onSubmit -= OnSubmit;
    }

    // Method to add an object as listener of the Main Menu System Events
    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        // Try to get hold of the "EventTrigger" component attached on ths gameobject
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();

        // Evaluates if we catched one to ignore this block, otherwise proceed inside
        if (trigger == null) 
        {
            // Automatically add the component missing and hold a reference to it 
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // Subscribes the interactable UI to UI event triggers
        AddSelectEventBehaviour(trigger);
        AddDeselectEventBehaviour(trigger);
        AddOnPointerEnterEventBehaviour(trigger);
        AddOnPointerExitEventBehaviour(trigger);
    }

    // ------------------------- ATTACHER METHODS ---------------------------

    // Method to attach a Selection event response to an interactable UI object
    protected virtual void AddSelectEventBehaviour(EventTrigger triggerable) 
    {
        // Initialized an "onSelect" event and stores it to a temp variable
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.Select
        };

        // Entasked the event of what to call once executed
        SelectEntry.callback.AddListener(OnSelect);

        // Subscribes this interactable UI object to the initialized event
        triggerable.triggers.Add(SelectEntry);
    }

    // Method to attach a Deselection event response to an interactable UI object
    protected virtual void AddDeselectEventBehaviour(EventTrigger triggerable)
    {
        // Initialized an "onDeselect" event and stores it to a temp variable
        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry {
            eventID = EventTriggerType.Deselect
        };

        // Entasked the event of what to call once executed
        DeselectEntry.callback.AddListener(OnDeselect);

        // Subscribes this interactable UI object to the initialized event
        triggerable.triggers.Add(DeselectEntry);
    }

    // Method to attach a MouseHoverEnter event response to an interactable UI object
    protected virtual void AddOnPointerEnterEventBehaviour(EventTrigger triggerable)
    {
        // Initialized an "onPointerEnter" event and stores it to a temp variable
        EventTrigger.Entry PointerEnter = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerEnter
        };

        // Entasked the event of what to call once executed
        PointerEnter.callback.AddListener(OnPointerEnter);

        // Subscribes this interactable UI object to the initialized event
        triggerable.triggers.Add(PointerEnter);
    }

    // Method to attach a MouseHoverExit event response to an interactable UI object
    protected virtual void AddOnPointerExitEventBehaviour(EventTrigger triggerable)
    {
        // Initialized an "onPointerExit" event and stores it to a temp variable
        EventTrigger.Entry PointerExit = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerExit
        };

        // Entasked the event of what to call once executed
        PointerExit.callback.AddListener(OnPointerExit);

        // Subscribes this interactable UI object to the initialized event
        triggerable.triggers.Add(PointerExit);
    }

    // ------------------------- UI BEHAVIOURS ---------------------------

    // Method that called when a Selection Event happens
    public void OnSelect(BaseEventData eventData)
    {
        // Executes this Event in the Inspector 
        onSelect?.Invoke();

        // Set's this object as the last selected interactable UI
        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        // Skips animating this interactable UI if it is included in the "_animationExclusions" list
        if (_animationExclusions.Contains(eventData.selectedObject)) return;

        // Animates the scale of this interactable UI
        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
        _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }

    // Method that called when a Deselection Event happens
    public void OnDeselect(BaseEventData eventData)
    {
        // Skips animating this interactable UI if it is included in the "_animationExclusions" list
        if (_animationExclusions.Contains(eventData.selectedObject)) return;

        // Animates the scale of this interactable UI
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>(); 
        _scaleDownTween = eventData.selectedObject.transform.DOScale(_scales[sel], _scaleDuration);
    }

    // Method that called when a MousePointingEnter Event happens
    public void OnPointerEnter(BaseEventData eventData)
    {
        // Holds a reference to the PointerEvent, a child class of BaseEventData
        PointerEventData pointerEventData = eventData as PointerEventData;

        // Only proceeds if we retrieved a pointerEvent reference
        if (pointerEventData != null) 
        {
            // Tries finding a selectable in the Parent gameObject
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();

            // Only proceeds if there's none in the parent
            if (sel == null) 
            {
                // Look for a selectable from the child objects instead
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }

            // Selects this interactable UI
            pointerEventData.selectedObject = sel.gameObject;
        }
    }

    // Method that called when a MousePointingExit Event happens
    public void OnPointerExit(BaseEventData eventData)
    {
        // Holds a reference to the PointerEvent, a child class of BaseEventData
        PointerEventData pointerEventData = eventData as PointerEventData;

        // Only proceeds if there's pointerEvent found
        if (pointerEventData != null)
        {
            // Clears the interactable UI from the selection 
            pointerEventData.selectedObject = null;
        }
    }

    // ------------------------- UI INTERACTIONS ---------------------------
    
    // Method to navigate through interactable UI objects
    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        // Prevents deselection when there's no new selection 
        if (EventSystem.current.currentSelectedGameObject == null &&
            _lastSelected != null) 
        {
            // Selects the last interactable UI selected instead
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }
    
    // Method to interact the selected button
    protected virtual void OnSubmit(InputAction.CallbackContext context)
    {
        // Tries retrieving the current selected interactable UI
        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        // Only proceeds further if there's even a selection happening 
        if (selectedObj == null) return; 

        // Retrieves the button of the selected interactable UI (to ensure that it is a button)
        Button button = selectedObj.GetComponent<Button>();
        
        // Only proceeds if the selected object is actually a Button
        if (button == null) return;

        // Executes the button's click
        button.onClick.Invoke();
    }

    // ------------------------- HELPER METHODS ---------------------------

    // Coroutine Method to add delay before UI selection
    protected virtual IEnumerator SelectAfterDelay() 
    {
        // The waiting time before execution
        yield return new WaitForSeconds(1f);

        // Automatically selects the specified interactable UI set in the Inspector
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }
}