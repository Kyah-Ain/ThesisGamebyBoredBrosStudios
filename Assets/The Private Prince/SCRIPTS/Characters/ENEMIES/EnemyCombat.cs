using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

public class EnemyCombat : CombatManager
{
    // ------------------------- VARIABLES -------------------------

    // Add variables here when needed...

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public override void Update()
    {
        // Calls from the parent class (CombatManager)
        base.Update();
    }
}