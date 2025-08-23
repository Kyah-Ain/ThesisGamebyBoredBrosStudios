using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor
        Cursor.visible = false; // Hides the cursor 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Accepts "Esc" key input from the keyboard
        {
            Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
            Cursor.visible = true; // Shows the cursor
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Locks the cursor
            Cursor.visible = false; // Hides the cursor
        }
    }
}
