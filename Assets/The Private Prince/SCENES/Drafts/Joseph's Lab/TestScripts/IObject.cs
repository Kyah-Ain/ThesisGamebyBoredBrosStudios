using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class IObject : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public abstract void Interact();

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Player"))
            collision.GetComponent<CharacterController3D>().OpenInteractableIcon();
    }

    private void OnTriggerExit(Collider collision)
    {
        if(collision.CompareTag("Player"))
            collision.GetComponent<CharacterController3D>().CloseInteractableIcon();
    }
}
