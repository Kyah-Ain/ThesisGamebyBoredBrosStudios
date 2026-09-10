using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class Collectibles : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public UnityEvent onCollision;
    
    // ----------------------- UNITY METHODS -------------------------
    
    // OnTriggerEnter is called when this script's object collide with another object
    protected virtual void OnTriggerEnter(Collider actor)
    {
        if (actor.gameObject.CompareTag("Player"))
        {    
            onCollision?.Invoke();
            
            Destroy(this.gameObject);
        };
    }
}
