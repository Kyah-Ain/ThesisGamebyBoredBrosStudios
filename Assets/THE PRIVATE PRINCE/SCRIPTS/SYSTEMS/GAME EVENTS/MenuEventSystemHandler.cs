//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using UnityEngine.Events; // NOTE ~ Unity Event Method (1)
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.UI;
//using DG.Tweening;

//// Requires a DebuggerKey.cs for it to debug, otherwise it will be muted
//[RequireComponent(typeof(DebuggerNiAinPjls))]

//public class MenuEventSystemHandler : MonoBehaviour
//{
//    // ------------------------- VARIABLES -------------------------

//    [Header("EVENTS")]
//    [SerializeField] protected UnityEvent onSelect; // NOTE ~ Unity Event Method (2)
//    //[SerializeField] protected GameEvent onSelect; // NOTE ~ Ain Method

//    [Header("CONTROLS")]
//    protected ManananggalControls manananggalControls;

//    [Header("REFERENCE")]
//    [SerializeField] DebuggerNiAinPjls debuggerNiAin;

//    [Header("DATA")]
//    public List<Selectable> Selectables = new List<Selectable>();
//    [SerializeField] protected Selectable _firstSelected;
//    protected Selectable _lastSelected;

//    // Container for the original/default scales of each UI elements before animation (for reset purpose)
//    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

//    [Header("ANIMATIONS")]
//    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();
//    protected Tween _scaleUpTween;
//    protected Tween _scaleDownTween;
//    [SerializeField] protected float _selectedAnimationScale = 1.1f;
//    [SerializeField] protected float _scaleDuration = 0.25f;

//    // ----------------------- UNITY METHODS -------------------------

//    // Awake is called when the script instance is being loaded
//    public virtual void Awake()
//    {
//        // Try to find optional DebuggerComponent on this GameObject
//        debuggerNiAin = GetComponent<DebuggerNiAinPjls>();

//        // Iterates through each UI inserted the list
//        foreach (var selectable in Selectables) 
//        {
//            // Coonverts a simple UI to a navigateable with keys type of UI 
//            AddSelectionListeners(selectable);

//            // Stores the UI's original/initial size before animation
//            _scales.Add(selectable, selectable.transform.localScale);  
//        }

//        // Evaluates if an InputManager instance exists in the scene (for reference)
//        if (InputManager.Instance == null)
//        {
//            debuggerNiAin.Log("InputManager Instance is NULL!");

//            return;
//        }

//        // Automatically sets the 'Initialized' input control maps from InputManager 
//        if (InputManager.Instance.Controls == null)
//        {
//            debuggerNiAin.Log("InputManager Controls is NULL!");

//            return;
//        }

//        // Prepared the controls to be ready for use  
//        manananggalControls = InputManager.Instance.Controls;

//        debuggerNiAin.Log($"New Input System was set: {manananggalControls}");
//    }

//    // OnEnable is called when the object becomes enabled and active
//    public virtual void OnEnable() 
//    {
//        Subscribe();

//        // Iterates through each UI inserted the list
//        for (int i = 0; i < Selectables.Count; i++)
//        {
//            // Resets the UI back to its original/initial size before animation
//            Selectables[i].transform.localScale = _scales[Selectables[i]];
//        }

//        StartCoroutine(SelectAfterDelay());
//    }

//    // OnEnable is called when the object becomes disabled and inactive
//    void OnDisable()
//    {
//        Unsubscribe();

//        _scaleUpTween.Kill(true);
//        _scaleDownTween.Kill(true);
//    }

//    // OnDestroy is called when the object is destroyed
//    void OnDestroy()
//    {
//        Unsubscribe();

//        _scaleUpTween.Kill(true);
//        _scaleDownTween.Kill(true);
//    }

//    // ----------------------- SUBSCRIPTION METHODS -------------------------

//    // Method to subscribe to events as a listener
//    public void Subscribe()
//    {
//        // Proceeds only if the input control reference was successfully set
//        if (manananggalControls == null) return;

//        // SUBSCRIBE METHODS to the input action events
//        manananggalControls.UI.Navigate.performed += OnNavigate;
//    }

//    // Method to unsubscribe from events 
//    public void Unsubscribe()
//    {
//        // Proceeds only if the input control reference was successfully set
//        if (manananggalControls == null) return;

//        // UNSUBSCRIBE METHODS to the input action events
//        manananggalControls.UI.Navigate.performed -= OnNavigate;
//    }

//    // Method to add an object as listener of the Main Menu System Events
//    protected virtual void AddSelectionListeners(Selectable selectable)
//    {
//        // Try to get hold of the "EventTrigger" component attached on ths gameobject
//        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();

//        // Evaluates if we catched one to ignore this block, otherwise proceed inside
//        if (trigger == null) 
//        {
//            // Automatically add the component missing and hold a reference to it 
//            trigger = selectable.gameObject.AddComponent<EventTrigger>();
//        }

//        // ...
//        AddSelectEventBehaviour(trigger);

//        // ...
//        AddDeselectEventBehaviour(trigger);

//        // ...
//        AddOnPointerEnterEventBehaviour(trigger);

//        // ...
//        AddOnPointerExitEventBehaviour(trigger);
//    }

//    // ------------------------- ATTACHER METHODS ---------------------------

//    // Method to attach ...
//    protected virtual void AddSelectEventBehaviour(EventTrigger triggerable) 
//    {
//        // Initialized an "onSelect" event 
//        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
//        {
//            // ...
//            eventID = EventTriggerType.Select
//        };

//        // ...
//        SelectEntry.callback.AddListener(OnSelect);

//        // ...
//        triggerable.triggers.Add(SelectEntry);
//    }

//    // ...
//    protected virtual void AddDeselectEventBehaviour(EventTrigger triggerable)
//    {
//        // Initialized an "onDeselect" event 
//        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
//        {
//            // ...
//            eventID = EventTriggerType.Deselect
//        };

//        // ...
//        DeselectEntry.callback.AddListener(OnDeselect);

//        // ...
//        triggerable.triggers.Add(DeselectEntry);
//    }

//    // ...
//    protected virtual void AddOnPointerEnterEventBehaviour(EventTrigger triggerable)
//    {
//        // Initialized an "onPointerEnter" event 
//        EventTrigger.Entry PointerEnter = new EventTrigger.Entry
//        {
//            // ...
//            eventID = EventTriggerType.PointerEnter
//        };

//        // ...
//        PointerEnter.callback.AddListener(OnPointerEnter);

//        // ...
//        triggerable.triggers.Add(PointerEnter);
//    }

//    // ...
//    protected virtual void AddOnPointerExitEventBehaviour(EventTrigger triggerable)
//    {
//        // Initialized an "onPointerExit" event 
//        EventTrigger.Entry PointerExit = new EventTrigger.Entry
//        {
//            // ...
//            eventID = EventTriggerType.PointerExit
//        };

//        // ...
//        PointerExit.callback.AddListener(OnPointerExit);

//        // ...
//        triggerable.triggers.Add(PointerExit);
//    }

//    // ------------------------- UI BEHAVIOURS ---------------------------

//    // ...
//    public void OnSelect(BaseEventData eventData)
//    {
//        // Plays SFX
//        onSelect?.Invoke(); // NOTE ~ Unity Event Method (3)
//        //onSelect?.TriggerEvent(); // NOTE ~ Ain Method

//        // ...
//        _lastSelected = eventData.selectedObject.GetComponent<Selectable>();

//        // ...
//        if (_animationExclusions.Contains(eventData.selectedObject)) return;

//        // ...
//        Vector3 newScale = eventData.selectedObject.transform.localScale * _selectedAnimationScale;
//        _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
//    }

//    // ...
//    public void OnDeselect(BaseEventData eventData)
//    {
//        // ...
//        if (_animationExclusions.Contains(eventData.selectedObject)) return;

//        // ...
//        Selectable sel = eventData.selectedObject.GetComponent<Selectable>(); 
//        _scaleDownTween = eventData.selectedObject.transform.DOScale(_scales[sel], _scaleDuration);
//    }

//    // ...
//    public void OnPointerEnter(BaseEventData eventData)
//    {
//        // ...
//        PointerEventData pointerEventData = eventData as PointerEventData;

//        // ...
//        if (pointerEventData != null) 
//        {
//            // ...
//            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();

//            // ...
//            if (sel == null) 
//            {
//                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
//            }

//            // ...
//            pointerEventData.selectedObject = sel.gameObject;
//        }
//    }

//    // ...
//    public void OnPointerExit(BaseEventData eventData)
//    {
//        // ...
//        PointerEventData pointerEventData = eventData as PointerEventData;

//        // ...
//        if (pointerEventData != null)
//        {
//            // ...
//            pointerEventData.selectedObject = null;
//        }
//    }

//    // ...
//    protected virtual void OnNavigate(InputAction.CallbackContext context)
//    {
//        // Evaluates ...
//        if (EventSystem.current.currentSelectedGameObject == null &&
//            _lastSelected != null) 
//        {
//            // ...
//            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
//        }
//    }

//    // ------------------------- HELPER METHODS ---------------------------

//    // COROUTINE Method ...
//    protected virtual IEnumerator SelectAfterDelay() 
//    {
//        // ...
//        yield return null;

//        // ...
//        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
//    }
//}