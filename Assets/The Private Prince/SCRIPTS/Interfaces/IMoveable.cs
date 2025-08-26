public interface IMoveable 
{
    // ------------------- REQUIREMENT VARIABLES -------------------------

    public float iWalkingSpeed { get; set; }
    public float iGravityWeight { get; set; }

    // ------------------------- CONTRACTS -------------------------

    public void HandleInput();
}