using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] private CharacterController characController;
    [SerializeField] private Animator animatorController;

    [Header("MOVEMENT")]
    [SerializeField] private float movementSpeed = 6f;

    /*
    [SerializeField] private float turningSpeed = 0.1f;
    [SerializeField] private float turningVelocity;
    */

    // ------------------------- UNITY METHODS -------------------------

    // Awake is called before all frame updates
    private void Awake()
    {
        // Evaluates if there's no existing "Character Controller" component on the object
        if (characController == null)
        {
            // Assigns the gameObject's "Character Controller" autmatically to this script
            characController = GetComponent<CharacterController>();

            // Assigns the gameObject's "Animator Controller" automatically to this script
            animatorController = GetComponent<Animator>();

            Debug.Log($"Character Controller was set: {characController}");
        }
        else 
        {
            Debug.LogError("ASSIGN A CHARACTER CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Method Calls
        Move();
    }

    // ------------------------- DEV METHODS -------------------------

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
            // - ("Name of the Animation Parameter", player.input, transition smoothness, counter)
            animatorController.SetFloat("Input Magnitude", direction.magnitude, 0.05f, Time.deltaTime);

            // Computes the angle needed to rotate the character to the direction it's moving
            // - "Mathf.Atan2" calculates the angle needed to rotate from 0 up to the target x & z coordinate
            // - "Mathf.Rad2Deg" converts the Rad computed value of "Atan2" into degrees
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            /*
            // Smooths the character rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turningVelocity, turningSpeed);
            */

            // Applies the computed rotation to the gameObject's rotation (Rotates the gameObject)
            // "Quaternion.Euler" to avoid gimbal locking or wrong rotation starting position
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Controls the "Character Controller" of a Unity game object
            characController.Move(direction * movementSpeed * Time.deltaTime);
        }
        else 
        {
            // Animates the character when NOT moving
            // - ("Name of the Animation Parameter", player.input, transition smoothness, counter)
            animatorController.SetFloat("Input Magnitude", 0f, 0.05f, Time.deltaTime);
        }
    }

    /*
    // Method for Character Animation
    public void Animate() 
    {

    }
    */
}