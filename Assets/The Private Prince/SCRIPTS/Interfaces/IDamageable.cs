public interface IDamageable
{
    // ------------------- REQUIREMENT VARIABLES -------------------------

    public float iHealth { get; set; }
    public float iMaxHealth { get; set; }

    public bool iBlock { get; set; }
    //public bool iEvade { get; set; }

    //public float iDefense { get; set; }

    //public int iAttackDamage { get; set; }

    // ------------------------- CONTRACTS -------------------------

    public void Die();
    //public void TakeDamage();
    public void TakeDamage(int damage);
    public void Heal();
}