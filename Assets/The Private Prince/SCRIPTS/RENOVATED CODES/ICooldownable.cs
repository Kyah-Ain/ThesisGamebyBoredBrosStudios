using UnityEngine;

public interface ICooldownable
{
    // ------------------- REQUIREMENT VARIABLES -------------------------

    public bool isCooldown { get; set; }
    public float cooldown { get; set; }

    // ------------------------- CONTRACTS -------------------------

    public bool isInCooldown(float duration);
}
