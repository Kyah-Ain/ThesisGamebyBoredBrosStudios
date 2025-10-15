using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public QuestEvents questEvents;

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.LogError("Found more than one Game Events Manager in the screen.");
        }
        Instance = this;

        //  Initializes the events
        questEvents = new QuestEvents();
    }  
}


