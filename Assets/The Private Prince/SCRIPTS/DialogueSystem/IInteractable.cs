// Interface that defines interactable objects in the game
public interface IInteractable
{
    // Method that must be implemented by any interactable object
    void Interact(PlayerController3D player); // Takes the player as parameter to handle interaction
}