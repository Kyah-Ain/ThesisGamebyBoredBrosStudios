using UnityEngine;

public interface IAlertable
{
    // ------------------- REQUIREMENT VARIABLES -------------------------

    public bool IBeenAlerted { get; set; }
    //public enum EnemyState { Neutral, Chase } // Different states this AI can be in

    // ------------------------- CONTRACTS -------------------------

    void Neutral();

    void Chase(Transform targetChase);

    //void ForcedAlert(Transform targetChase);

    //void SwitchState(EnemyState newState);

}