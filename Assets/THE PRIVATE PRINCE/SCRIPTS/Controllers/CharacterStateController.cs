using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using TMPro;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

public class CharacterStateController : MonoBehaviour
{
    // // ------------------------- VARIABLES -------------------------

    // public CharacterController CharacController => characController;
    // public Animator AnimatorController => animatorController;
    // public GameObject SpriteRoot => spriteRoot;

    // [Header("BOOLEANS")]
    
    
    // public bool canAttack = true; // Indicates if the player can perform an attack
    // public bool canMove = true; // Indicates if the player can move
    // public bool hasHit = false; // Indicates if the player has hit something with its attack
    // public bool isBlocking = false; //  Indicates if the player is currently blocking an attack with their shield

    // // ------------------------- UNITY METHODS -------------------------
    // #region UNITY LOGICS

    // // Awake is called before all frame updates
    // private void Awake()
    // {
        
    // }

    // // Update is called once per frame
    // // NOTE: Set Script Execution Order so this runs BEFORE PlayerMovement2Point5D and
    // // PlayerCombat2Point5D — they depend on ApplyGravity() and InDialogue() having run first.
    // private void Update()
    // {


    //     // ApplyGravity();

    //     InDialogue();
    // }

    // #endregion

    // public void SetCharacMood(string mood)
    // {
    //     switch (mood)
    //     {
    //         case "Blocking":
    //             isBlocking = true;

    //             // Safe check for IDamageable
    //             IDamageable thisDamageable = this.GetComponent<IDamageable>();
    //             if (thisDamageable != null)
    //                 thisDamageable.iVulnerable = false;

    //             canMove = false;
    //             canAttack = false;
    //             break;

    //         case "Attacking":
    //             canAttack = false;
    //             canMove = false;
    //             isBlocking = false;
    //             break;

    //         default:
    //             Debug.LogWarning($"Unknown mood set: {mood}");
    //             break;
    //     }
    // }

    // public void ResetCharacMood(string mood)
    // {
    //     switch (mood)
    //     {
    //         case "Blocking":
    //             Debug.Log("Player block reset!");

    //             isBlocking = false;

    //             IDamageable thisDamageable = this.GetComponent<IDamageable>();
    //             if (thisDamageable != null)
    //                 thisDamageable.iVulnerable = true;

    //             canMove = true;
    //             canAttack = true;
    //             break;

    //         case "Attacking":
    //             Debug.Log("Player attack reset!");

    //             canAttack = true;
    //             canMove = true;
    //             isBlocking = false;
    //             break;

    //         case "Moving":
    //             Debug.Log("Player movement reset!");

    //             canMove = true;
    //             canAttack = true;
    //             isBlocking = false;
    //             break;

    //         default:
    //             Debug.LogWarning($"Unknown mood reset: {mood}");
    //             break;
    //     }
    // }

    // #endregion

    // // --------------------------- GRAVITY ---------------------------
    // #region GRAVITY LOGICS

    // // Method for applying gravity to the character to simulate freefall and grounded movement
    // public virtual void ApplyGravity()
    // {
    //     if (characController == null) return;

    //     if (!characController.isGrounded)
    //     {
    //         Debug.Log("Player is Falling!");

    //         freefallVelocity += gravity * gravityMultiplier * Time.deltaTime;
    //         characController.Move(new Vector3(0, freefallVelocity, 0) * Time.deltaTime);
    //     }
    //     else
    //     {
    //         freefallVelocity = -1.0f; // Resets vertical velocity when grounded
    //     }
    // }

    // #endregion

    // // -------------------------- ANIMATIONS ---------------------------
    // #region ANIMATION LOGICS

    // // Method for Character Animation
    // public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    // {
    //     if (animatorController == null) return;
    //     animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    // }

    // // Method for Character Animation with bool parameters
    // public void AnimationSetbool(string paramaterName, bool boolState)
    // {
    //     if (animatorController == null) return;
    //     animatorController.SetBool(paramaterName, boolState);
    // }

    // #endregion
}