using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public QuestEvents questEvents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Found more than one Game Events Manager in the screen.");
            Destroy(gameObject);
            return;
        }


        Instance = this;
        //  Initializes the events
        questEvents = new QuestEvents();
    }  
}


