// -------------------------------------------------- From Sir Raz's Script --------------------------------------------------
using UnityEngine;

public class ConeViewCast : MonoBehaviour
{
    public Transform player;
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    void Update()
    {
        if (CanSeePlayer())
        {
            Debug.Log("AI sees the player!");
        }
        else
        {
            Debug.Log("Player is not visible.");
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > viewDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleMask))
        {
            return false;
        }

        return true;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 rightLimit = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewDistance);
    }
}