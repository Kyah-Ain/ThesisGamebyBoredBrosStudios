using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
 
// Pure combat: attack box cast + damage, blocking, and the input subscriptions for both.
// Depends on PlayerStateController for canAttack / isBlocking / mood / spriteRoot facing / animation calls.
[RequireComponent(typeof(CharacterStateController))]
public class CharacterCombat : MonoBehaviour
{
    // private CharacterStateController state;
 
    // [Header("COMBAT ATTRIBUTES")]
    // [SerializeField] private int attackDamage = 1; // Amount of damage dealt per attack
    // [SerializeField] protected float attackCooldown = 0.25f; // Amount of time between each attack
    // [SerializeField] protected float blockCooldown = 0f; // Amount of recovery time after blocking an attack
 
    // [Header("ATTACK CAST")]
    // [SerializeField] private Vector3 attackBoxCastSize = new Vector3(1f, 1f, 1f); // Defines the size of the attack box cast
    // [SerializeField] protected LayerMask exludedLayerMask; // Layer mask to filter unwanted targets
 
    // [Space(8f)] // Adds spacing in the Inspector
 
    // [SerializeField] private Transform raycastEmitter; // Point from which the raycast will be emitted
    // [SerializeField] private float raycastLength = 2f; // Defines how long the raycast would be
 
    // // Tracks previous block state to prevent continuous reset
    // private bool wasBlocking = false;
 
    // // Coroutine references to prevent multiple coroutines
    // private Coroutine attackCoroutine;
    // private Coroutine blockCoroutine;
 
    // // ------------------------- UNITY METHODS -------------------------
    // #region UNITY LOGICS
 
    // private void Awake()
    // {
    //     state = GetComponent<CharacterStateController>();
 
    //     // Validate raycast emitter
    //     if (raycastEmitter == null)
    //         Debug.LogWarning("Raycast Emitter is not assigned. Please assign a Transform for attack raycasts.");
    // }
 
    // private void OnEnable()
    // {
    //     // Ensure subscriptions are active when object is enabled
    //     if (state.Controls != null)
    //     {
    //         // Subscribes to the performed events
    //         state.Controls.Player.Attack.performed += NewAttack;
    //         state.Controls.Player.Block.performed += NewBlock;
    //         state.Controls.Player.Block.canceled += OnBlockReleased;
    //     }
    // }
 
    // private void OnDisable()
    // {
    //     // Clean up subscriptions when object is disabled
    //     if (state.Controls != null)
    //     {
    //         // Subscribes to the performed events
    //         state.Controls.Player.Attack.performed -= NewAttack;
    //         state.Controls.Player.Block.performed -= NewBlock;
    //         state.Controls.Player.Block.canceled -= OnBlockReleased;
    //     }
    // }
 
    // private void Start()
    // {
    //     // Initialize wasBlocking
    //     wasBlocking = false;
    // }
 
    // // Update is called once per frame
    // private void Update()
    // {
    //     if (state.InDialogue())
    //         return;
 
    //     // Check block state only when it changes
    //     float currentBlockValue = state.Controls?.Player.Block.ReadValue<float>() ?? 0f;
    //     if (currentBlockValue <= 0f && wasBlocking)
    //     {
    //         state.ResetCharacMood("Blocking");
    //         wasBlocking = false;
    //     }
    //     else if (currentBlockValue > 0f)
    //     {
    //         wasBlocking = true;
    //     }
    // }
 
    // private void OnDestroy()
    // {
    //     if (state.Controls != null)
    //     {
    //         // Unsubscribe from all events
    //         state.Controls.Player.Attack.performed -= NewAttack;
    //         state.Controls.Player.Block.performed -= NewBlock;
    //         state.Controls.Player.Block.canceled -= OnBlockReleased;
    //     }
 
    //     // Stop all coroutines
    //     StopAllCoroutines();
    // }
 
    // #endregion
 
    // // ---------------------------- COMBATS ---------------------------
    // #region COMBAT LOGICS
 
    // public virtual void Attack()
    // {
    //     if (!state.canAttack || state.inDialogue) return;
 
    //     Debug.Log("Player performed attack");
 
    //     state.SetCharacMood("Attacking");
 
    //     // Stop any existing attack coroutine
    //     if (attackCoroutine != null)
    //         StopCoroutine(attackCoroutine);
 
    //     attackCoroutine = StartCoroutine(AttackSequence(attackCooldown));
    // }
 
    // public virtual void NewAttack(InputAction.CallbackContext context)
    // {
    //     if (!state.canAttack || state.inDialogue || context.performed == false) return;
 
    //     // Calls the Animation that fires the attack animation
    //     state.AnimationSetbool("isAttacking", true);
 
    //     // Call Attack directly since we're using coroutine management there
    //     Attack();
    // }
 
    // // Coroutine for handling the attack sequence with delay and cooldown
    // protected IEnumerator AttackSequence(float cooldown)
    // {
    //     #region BOXCAST Detection Logic
 
    //     // Validate required components
    //     if (raycastEmitter == null || state.SpriteRoot == null)
    //     {
    //         Debug.LogError("Raycast Emitter or Sprite Root is missing! Cannot perform attack.");
    //         yield break;
    //     }
 
    //     // Gets the half dimension of the full attack box size
    //     Vector3 halfExtents = attackBoxCastSize / 2f;
 
    //     // Sets the direction the character is facing
    //     bool isFacingLeft = state.SpriteRoot.transform.localScale.x < 0f;
    //     Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
    //     Quaternion boxRotation = this.transform.rotation;
 
    //     // Variable to store information about what the BoxCast has hit
    //     RaycastHit hitInfo;
 
    //     // Perform the box cast
    //     if (Physics.BoxCast(
    //         raycastEmitter.transform.position,
    //         halfExtents,
    //         attackDirection,
    //         out hitInfo,
    //         boxRotation,
    //         raycastLength,
    //         ~exludedLayerMask
    //     ))
    //     {
    //         // Get components from hit object
    //         IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
    //         IKnockable knockable = hitInfo.collider.GetComponent<IKnockable>();
 
    //         // Apply damage if possible
    //         if (damageable != null)
    //         {
    //             Debug.Log($"Enemy: {hitInfo.transform.name} HAS BEEN DAMAGED!");
    //             damageable.TakeDamage(attackDamage, false, this.transform);
 
    //             // Apply knockback if possible
    //             if (knockable != null)
    //             {
    //                 knockable.KnockBack(this.transform, hitInfo.transform);
    //             }
    //         }
    //     }
 
    //     // Visualizes the BoxCast in the Scene View for debugging
    //     DebugBoxCast.SimpleDrawBoxCast(raycastEmitter.transform.position, halfExtents, boxRotation, attackDirection, raycastLength, Color.red);
 
    //     #endregion
 
    //     // Cooldown duration before the player can attack again
    //     yield return new WaitForSeconds(cooldown);
 
    //     // Resets the attack animation state
    //     state.AnimationSetbool("isAttacking", false);
 
    //     // Resets the character's mood and states after the attack sequence is complete
    //     state.ResetCharacMood("Attacking");
 
    //     attackCoroutine = null;
    // }
 
    // // Method for Blocking Attacks Logic
    // public virtual void Block()
    // {
    //     Debug.Log("Player is Blocking!");
 
    //     if (state.isBlocking || state.inDialogue) return;
 
    //     state.SetCharacMood("Blocking");
 
    //     // Start block cooldown if needed
    //     if (blockCooldown > 0f)
    //     {
    //         if (blockCoroutine != null)
    //             StopCoroutine(blockCoroutine);
    //         blockCoroutine = StartCoroutine(BlockCooldownSequence(blockCooldown));
    //     }
    // }
 
    // public virtual void NewBlock(InputAction.CallbackContext context)
    // {
    //     if (!context.performed) return;
    //     Block();
    // }
 
    // private void OnBlockReleased(InputAction.CallbackContext context)
    // {
    //     // This will be called when the block button is released
    //     state.ResetCharacMood("Blocking");
    // }
 
    // // Coroutine for handling the blocking sequence with delay and cooldown
    // protected IEnumerator BlockCooldownSequence(float cooldown)
    // {
    //     // Shielding Cooldown duration after blocking an attack
    //     yield return new WaitForSeconds(cooldown);
 
    //     // Reset block state
    //     IDamageable thisDamageable = this.GetComponent<IDamageable>();
    //     if (thisDamageable != null)
    //         thisDamageable.iVulnerable = true;
 
    //     state.isBlocking = false;
    //     state.canAttack = true;
    //     state.canMove = true;
 
    //     blockCoroutine = null;
    // }
 
    // #endregion
}
