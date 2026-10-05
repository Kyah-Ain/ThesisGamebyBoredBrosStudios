using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic data structures like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

using UnityEngine.Events;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class QuestStateListener : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("EVENTS")]
    public UnityEvent onQuestCanStart;
    public UnityEvent onQuestStarted;
    public UnityEvent onQuestCanFinish;
    public UnityEvent onQuestEnded;
    
    [Header("REFERENCES")]
    [SerializeField] protected DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    
    [Header("QUEST")]
    [SerializeField] QuestInfoSO QuestInfo; // Container for the Scriptable Quest's Data
    [SerializeField][ReadOnly] string _questId; // Container for the questId we want this script to correlates to
    [SerializeField][ReadOnly] QuestState _currentQuestState; // Container for the quest State we want this script to correlates to
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }
        
        // Initialized the Quest it would track
        _questId = QuestInfo.id;
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        UnSubscribe();
    }
    
    // Start is called once before the first frame update
    void Start()
    {
        // Retrieves the Quest from the ID
        Ain.Quest quest = Ain.QuestManager.Instance.GetQuestById(_questId);
        
        // Receives an update to that particular Quest
        GameEventsManager.Instance.questEvents.QuestStateChange(quest);
    }
    
    #endregion
    
    // ------------------------- SUBSCRIPTIONS ------------------------
    #region SUBSCRIPTIONS

    // Method to subscribe your local method to an event trigger
    void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    // Method to UnSubscribe your local method to an event trigger
    void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.questEvents.onQuestStateChange -= QuestStateChange;
    }
    
    // Method to update the Quest State for this point 
    void QuestStateChange(Ain.Quest quest)
    {
        // Ignore Quest updates that aren't intended for this NPC
        if (!quest.info.id.Equals(_questId))
        {
            return;
        }
        
        // Update the state of this from the passed state value from the caller
        _currentQuestState = quest.state;

        // Chooses the suitable case base on the parameter passed
        switch (_currentQuestState)
        {
            // Dialogue logic for No Quest given
            case QuestState.REQUIREMENTS_NOT_MET:
                
                break;
                
            // Dialogue logic for when there's a Quest wants to be given
            case QuestState.CAN_START:
                onQuestCanStart?.Invoke();
                break;
                
            // Dialogue logic for when there's a Quest waiting to be fulfilled
            case QuestState.IN_PROGRESS:
                onQuestStarted?.Invoke();
                break;
                
            // Dialogue logic for when there's a Quest waiting to be finished
            case QuestState.CAN_FINISH:
                onQuestCanFinish?.Invoke();
                break;
                
            // Dialogue logic for when finished a Quest
            case QuestState.FINISHED:
                onQuestEnded?.Invoke();
                break;
        }
    }
    
    #endregion
}
