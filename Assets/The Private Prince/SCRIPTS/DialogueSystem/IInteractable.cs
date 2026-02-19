// Interface that defines interactable objects in the game
public interface IInteractable
{
    // Method that must be implemented by any interactable object
    void Interact(DialogueInteraction player); // Takes the player as parameter to handle interaction
}