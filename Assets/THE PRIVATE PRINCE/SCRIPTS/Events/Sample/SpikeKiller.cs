using UnityEngine;

public class SpikeKiller : GameEventTrigger
{
    // ------------------------- VARIABLES -------------------------

    [Header("OPTIONAL")]
    [SerializeField] string levelToLoad;

    // ----------------------- UNITY METHODS -------------------------
    private void OnTriggerEnter2D(Collider2D actor)
    {
        if (actor.CompareTag("Player")) 
        {
            if (levelToLoad.Length > 0) 
            {
                base.ExecuteEvents(levelToLoad);
            }

            base.ExecuteEvents();
        }
    }
}
