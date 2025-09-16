using UnityEngine;

// Creates a menu option in Unity's Create menu for making DialogueObject assets
[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    [SerializeField][TextArea] private string[] dialogue; // Array of dialogue text with text area attribute for easier editing

    [SerializeField] private Response[] responses; // Array of possible responses for this dialogue

    public string[] Dialogue => dialogue; // Public property to access dialogue array (read-only)

    public bool HasResponses => Responses != null && Responses.Length > 0; // Checks if this dialogue has any responses

    public Response[] Responses => responses; // Public property to access responses array (read-only)
}