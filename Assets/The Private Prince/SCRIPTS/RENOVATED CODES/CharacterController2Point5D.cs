using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

[RequireComponent(typeof(CharacterController))] // Requires this GameObject to have a CharacterController component in order to function properly

public class CharacterController2Point5D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animatorController; // Reference to the Animator component for controlling character animations
    [SerializeField] private SpriteRenderer spriteRenderer; // ...

    [Header("AI ATTRIBUTES")]
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves

    [Header("INTERACTIONS")]
    [SerializeField] private GameObject interactIcon; // Icon that will pop up when near interactable object

    private Vector3 boxSize = new Vector3(0.1f, 1f, 0.5f); // -joseph

    // ------------------------- UNITY METHODS -------------------------

    // Awake is called before all frame updates
    private void Awake()
    {
        // Evaluates if there's no existing "Character Controller" component on the object
        if (characController == null)
        {
            Debug.Log($"Character Controller was set: {characController}");
            
            // Assigns the gameObject's "Character Controller" autmatically to this script
            characController = GetComponent<CharacterController>();

            // Assigns the gameObject's "Animator Controller" automatically to this script
            animatorController = GetComponent<Animator>();

            // ...
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("ASSIGN A CHARACTER CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }
    }

    // Start is called once the script is loaded 
    private void Start()
    {
        interactIcon.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        // Calls the method that handles character movement
        Move();

        // Interact Key
        if (Input.GetKeyUp(KeyCode.E))
            CheckInteraction();
    }

    // ------------------------- DEV METHODS -------------------------

    public void OpenInteractableIcon() // - joseph
    {
        interactIcon.SetActive(true);
    }

    public void CloseInteractableIcon() // - joseph
    {
        interactIcon.SetActive(false);
    }

    private void CheckInteraction() // - joseph
    {
        Collider[] hits = Physics.OverlapBox(transform.position, boxSize / 2f, transform.rotation);

        if (hits.Length > 0)
        {
            foreach (Collider c in hits)
            {
                if (c.transform.GetComponent<IObject>())
                {
                    c.transform.GetComponent<IObject>().Interact();
                }
            }
        }
    }

    // Method for Character Movement Logic
    public void Move()
    {
        // Get's the Horizontal & Vertical Value from Unity's Input System
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Computes for the direction by merging horizontal & vertical positions
        // - ".normalized" so that moving diagonally would make us not move faster
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Evaluates if there is a movement
        // ".magnitude" to compute for the distance 
        if (direction.magnitude >= 0.1f)
        {
            // Animates the character when moving
            //Animate("Input Magnitude", direction.magnitude, 0.05f, Time.deltaTime);

            // Controls the "Character Controller" of a Unity game object
            characController.Move(direction * movementSpeed * Time.deltaTime);

            // ...
            if (horizontal < 0f)
            {
                spriteRenderer.flipX = true;
            }
            else 
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Animates the character when NOT moving
            //Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);
        }
    }

    // -------------------------- ANIMATIONS ---------------------------

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        // - ("Name of the Animation Parameter", player.input value, transition smoothness, counter)
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }
}
