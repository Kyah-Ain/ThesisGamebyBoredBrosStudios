using UnityEngine;

public interface IKnockable
{
    // ------------------------- CONTRACTS -------------------------

    public void KnockBack(Transform objectKnocker, Transform knockableObject);
}