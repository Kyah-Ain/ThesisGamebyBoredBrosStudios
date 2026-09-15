using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/DialogueByAin")]
public class DialogueLines : ScriptableObject
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("DialogueLines")]
    [TextArea] public string[] speechLines;
}
