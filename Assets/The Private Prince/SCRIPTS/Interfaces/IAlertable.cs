using UnityEngine;

//public enum EnemyState { Neutral, Chase } // Different states this AI can be in

public interface IAlertable
{
    // ------------------------- CONTRACTS -------------------------
    void Chase(Transform targetChase);

    //void SwitchState(EnemyState newState);

    //public void HardResetAlert();
}