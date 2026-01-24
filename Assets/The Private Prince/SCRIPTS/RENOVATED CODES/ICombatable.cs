using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

public interface ICombatable
{
    // ------------------------- CONTRACTS -------------------------

    public void Attack();
    public void Block();
}