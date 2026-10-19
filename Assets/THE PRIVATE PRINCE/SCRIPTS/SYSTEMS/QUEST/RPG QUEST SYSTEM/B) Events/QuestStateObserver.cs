using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class QuestStateObserver : MonoBehaviour
{
    // // ------------------------- VARIABLES -------------------------
    // [Header("EVENTS")]
    // public UnityEvent onObserveableComplete;
    //
    // [Header("REFERENCES")]
    // [SerializeField] protected DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    //
    // [Header("QUEST INFOS")]
    // [SerializeField] QuestInfoSO _questInfoSO; // Container for the Scriptable Quest's Data
    // [SerializeField, ReadOnly] string _questId; // Container for the questId we want this script to correlates to
    //
    // [Header("OBSERVER SETTINGS")]
    // [SerializeField] QuestStep[] stepsToObserve; // Refers to Quest Step/s to observe
    // [SerializeField, ReadOnly] int currentStepObserving = 0; // Refers to Quest Step being observed
    //
    // // ----------------------- PACKED METHODS -------------------------
    //
    // // Retrieves the Quest's data using its Quest id
    // private Ain.Quest QuestData => Ain.QuestManager.Instance.GetQuestById(_questInfoSO.id);
    //
    // // ----------------------- UNITY METHODS -------------------------
    // #region UNITY METHODS
    //
    // // Awake is called when this script was first initialized & loaded
    // void Awake()
    // {
    //     #region INITIALIZATIONS
    //     
    //     // Checks if our "Debugger" reference was null or not been set
    //     if (debuggerNiAin == null)
    //     {
    //         // Automatically set the component by looking for the script from this object
    //         debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
    //         
    //         // Logs a missing component if still not found
    //         if (debuggerNiAin == null) MissingComponentLog("debuggerNiAin");
    //     }
    //     
    //     #endregion
    //     
    //     // Initialized the Quest it would track
    //     _questId = _questInfoSO.id;
    // }
    //
    // // OnEnable is called when the object becomes enabled and active
    // void OnEnable()
    // {
    //     
    // }
    //
    // // OnDisable is called when the object becomes disabled
    // void OnDisable()
    // {
    //     
    // }
    //
    // // Start is called once before the first frame update
    // void Start()
    // {
    //     
    // }
    //
    // // Update is called once per frame
    // private void Update()
    // {
    //     // ...
    //     if (IsStepCompleteFor(currentStepObserving))
    //     {
    //         onObserveableComplete?.Invoke();
    //     }
    // }
    //
    // #endregion
    //
    // // --------------------- OBSERVER METHODS -------------------------
    // #region OBSERVER METHODS
    //
    // // Method to determine if the step observing has been complete
    // bool IsStepCompleteFor(int stepObserving)
    // {
    //     // ...   
    //     if (QuestData.currentQuestStepIndex == stepObserving)
    //     {
    //         return false;
    //     }
    //
    //     // Proceed
    //     currentStepObserving = (currentStepObserving + 1) % stepsToObserve.Length;
    //
    //     return true;
    // }
    //
    // #endregion
    //
    // // ------------------------- DEBUGGERS -------------------------
    // #region DEBUGGERS
    //
    // // Method to log missing components
    // void MissingComponentLog(string component)
    // {
    //     Debug.LogError($"MISSING COMPONENT: Cannot find the \"{component}\"");
    // }
    //
    // #endregion
}
