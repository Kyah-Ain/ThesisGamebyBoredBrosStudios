public interface IDamageable
{
    // ------------------- REQUIREMENT VARIABLES -------------------------

    public float iHealth { get; set; }
    public float iDefense { get; set; }

    // ------------------------- CONTRACTS -------------------------
    
    public void Die();
    public void TakeDamage(int damage);
    public void Heal();
}
