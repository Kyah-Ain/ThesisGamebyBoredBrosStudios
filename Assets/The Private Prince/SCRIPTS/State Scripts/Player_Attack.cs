using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Attack : StateMachineBehaviour
{
    // ------------------------- VARIABLES -------------------------

    private CharacterController2Point5D playerCharac;
    
    // -------------------------- UNITY METHODS --------------------------

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the CharacterController from the parent GameObject
        if (playerCharac == null)
        {
            // animator.transform is the child GameObject (where the Animator is)
            // .parent gets the parent GameObject (where your CharacterController is)
            playerCharac = animator.transform.parent.GetComponent<CharacterController2Point5D>();
        }

        // Safety check to avoid NullReferenceException
        if (playerCharac != null)
        {
            playerCharac.canMove = false;
        }

        // ...
        animator.SetBool("isMoving", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Don't forget to re-enable movement when exiting the attack state
        if (playerCharac != null)
        {
            playerCharac.canMove = true;
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}