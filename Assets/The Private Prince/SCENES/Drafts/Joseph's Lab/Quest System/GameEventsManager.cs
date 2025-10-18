using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public QuestEvents questEvents;

    private void Awake()
    {
        // Check if another instance already exists in a persistent object
        GameEventsManager[] existingManagers = FindObjectsOfType<GameEventsManager>();

        foreach (GameEventsManager manager in existingManagers)
        {
            if (manager != this && manager.gameObject.scene.buildIndex == -1) // Persistent object
            {
                Debug.Log("Destroying duplicate GameEventsManager - one already exists");
                Destroy(gameObject);
                return;
            }
        }

        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            questEvents = new QuestEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


