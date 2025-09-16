// Interface that defines interactable objects in the game
public interface IInteractable
{
    // Method that must be implemented by any interactable object
    void Interact(Player2Point5D player); // Takes the player as parameter to handle interaction
}