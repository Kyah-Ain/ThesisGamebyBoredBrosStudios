using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroyOnLoadManager : MonoBehaviour
{
    // NOTE: INTHERIT FROM THIS FOR A MUCH MORE SAFEST APPROACH WITH NO CONFLICT TO TIMING
    // * only if you have your own DoNotDestroy logic on other script attached to the same object
    // * should also set the option to "PARENTS_AND_ROOT_ONLY" if this is the case 

    // ------------------------- VARIABLES -------------------------

    // ADD VARIABLES HERE IF NEEDED...
    public enum PersistenceScope
    {
        THIS_ONLY,        // Only this GameObject
        PARENTS_AND_ROOT_ONLY, // All parents and children, but NOT this object itself
        ALL     // This object, all parents, AND all children!
    }

    // Default Setting for Enum
    // * this ensures not to have a conflict with Destroy logics on other scripts
    public PersistenceScope appliesTo = PersistenceScope.PARENTS_AND_ROOT_ONLY;

    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    protected virtual void Awake()
    {
        switch (appliesTo) 
        {
            case PersistenceScope.THIS_ONLY:
                DontDestroyOnLoad(this.gameObject);
                Debug.Log($"Marked CURRENT object only: \"{this.gameObject.name}\"");
                break;

            case PersistenceScope.PARENTS_AND_ROOT_ONLY:
                DoNotDestroy(this.transform.parent);
                break;

            case PersistenceScope.ALL:
                DoNotDestroy(this.transform);
                break;
        }
    }

    // Method...
    public virtual void DoNotDestroy(Transform currentObj) 
    {
        // This 'While' loop iterates infinitely until there are no more parent gameObject
        // * which signifies that we've reached the root game object of this instance
        int loopIteration = 0;

        while (currentObj != null)
        {
            // * THIS IS IMPORTANT as DontDestroy on load only saves the Object set with it
            // * If this GameObject was excluded from destruction, its parent wont be unless specified
            // * Thats why we need to make the parent up to the root of this GameObject persitent to solve the issue

            Debug.Log($"Entered While Loop Iteration: {0}x");

            // Sets the current GameObject to not be destroyed on scene loads
            // * marking it to be persitent across scenes
            DontDestroyOnLoad(currentObj.gameObject);

            Debug.Log($"Successfully set {currentObj.gameObject.name} to become DontDestroyOnLoad!!!");
            Debug.Log($"Moving one heirachy now!");

            // Moves up one heirachy by setting the parent as now our current object
            currentObj = currentObj.parent;

            // Check if we've reached the root (parent is null)
            if (currentObj != null)
            {
                Debug.Log($"\"{currentObj.gameObject.name}\" is now the new current object to check " +
                    $"if its the root parent " +
                    $"or still a child of another Parent GameObject");

                // Increments the loop iteration count for DEVBUGGING PURPOSES ONLY
                loopIteration++;
            }
            else
            {
                Debug.Log("Reached the root GameObject, exiting loop ^ ^");
            }
        }

        Debug.Log($"Exits While Loop with total Iteration of: {loopIteration}x");
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDestroy()
    {
        Destroy(this.gameObject); // Destroy this gameObject when this script is destroyed (if it hasn't been marked as DontDestroyOnLoad)
    }
}