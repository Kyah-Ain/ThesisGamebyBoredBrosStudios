using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float secondsPerTrip = 40f;
    [SerializeField] private bool blendRotation = true;

    private void Update()
    {
        float t = Mathf.PingPong(Time.unscaledTime / secondsPerTrip, 1f);
        float eased = Mathf.SmoothStep(0f, 1f, t);

        transform.position = Vector3.Lerp(pointA.position, pointB.position, eased);

        if (blendRotation)
        {
            transform.rotation = Quaternion.Slerp(pointA.rotation, pointB.rotation, eased);
        }
    }
}