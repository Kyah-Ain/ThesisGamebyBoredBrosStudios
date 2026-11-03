using UnityEngine;

public class AIViewRotator : MonoBehaviour
{
    [SerializeField] AIMovement movement;
    [SerializeField] Transform emitter;      // Drag the SAME transform you used as AIDetection's visualEmmitter
    [SerializeField] float turnSpeed = 540f; // Degrees per second. 0 = instant snap

    void Awake()
    {
        if (movement == null) movement = GetComponent<AIMovement>();
    }

    void Update()
    {
        Vector2 f = movement.FacingDirection;
        Vector3 dir = new Vector3(f.x, 0f, f.y); // Xinput -> world X, Yinput -> world Z (same mapping as navAgent.velocity)

        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        emitter.rotation = turnSpeed <= 0f
            ? target
            : Quaternion.RotateTowards(emitter.rotation, target, turnSpeed * Time.deltaTime);
    }
}