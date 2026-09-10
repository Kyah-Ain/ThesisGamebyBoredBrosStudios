using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistanceTagger : MonoBehaviour
{
    // NOTE: INTHERIT FROM THIS FOR A MUCH MORE SAFEST APPROACH WITH NO CONFLICT TO TIMING
    // * only if you have your own DoNotDestroy logic on other script attached to the same object
    // * should also set the option to "PARENTS_AND_ROOT_ONLY" if this is the case 

    // ------------------------- VARIABLES -------------------------

    // ADD VARIABLES HERE IF NEEDED...
    public enum PersistenceScope
    {
        THIS_ONLY,        // Only this GameObject
        ALL     // This object, all parents, AND all children!
    }

    // Default Setting for Enum
    // * this ensures not to have a conflict with Destroy logics on other scripts
    public PersistenceScope appliesTo = PersistenceScope.ALL;

    // ------------------------- UNITY METHODS -------------------------

    // Built-in Unity method called when this script was first loaded
    protected virtual void Awake()
    {
        // Check the selected persistence scope and apply DontDestroyOnLoad accordingly
        if (appliesTo == PersistenceScope.THIS_ONLY) 
        {
            // Sets this GameObject to not be destroyed on scene loads (persitent across scenes)
            DontDestroyOnLoad(this.gameObject);

            Debug.Log($"Successfully set {this.gameObject} to become DontDestroyOnLoad!");

            return; // Exit early since we only want to make this specific GameObject persistent
        }

        // Sets this GameObject's root up to this GameObject to not be destroyed on scene loads (persitent across scenes)
        DontDestroyOnLoad(this.gameObject.transform.root);

        Debug.Log($"Successfully set {this.gameObject.transform.root.name} to become DontDestroyOnLoad!");
    }

    // Automated Unity Built-In method being called when this object is destroyed
    private void OnDestroy()
    {
        Destroy(this.gameObject); // Destroy this gameObject when this script is destroyed (if it hasn't been marked as DontDestroyOnLoad)
    }
}