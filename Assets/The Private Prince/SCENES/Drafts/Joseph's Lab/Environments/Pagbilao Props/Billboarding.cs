using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboarding : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;
    [SerializeField] private bool lockX; // If on, lock X rotation to original rotation
    [SerializeField] private bool lockY; // If on, lock Y rotation to original rotation
    [SerializeField] private bool lockZ; // If on, lock Z rotation to original rotation
    private Vector3 originalRotation;

    public enum BillboardType { LookAtCamera, CameraForward };

    private void Awake()
    {
        originalRotation = transform.rotation.eulerAngles; // Note the original rotation of the object upon starting the game
    }

    void LateUpdate() // LateUpdate ensures camera movement is processed before moving the sprites
    {
        // Calculate new rotations given the type of billboarding used
        switch (billboardType)
        {
            case BillboardType.LookAtCamera: // Stare at the camera's position
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward: // Stare directly parallel to the camera
                transform.forward = Camera.main.transform.forward;
                break;
            default: // Error case
                break;
        }
        // Modify the rotation in Euler space to lock certain dimensions
        Vector3 rotation = transform.rotation.eulerAngles;
        if (lockX) { rotation.x = originalRotation.x; }
        if (lockX) { rotation.x = originalRotation.x; }
        if (lockX) { rotation.x = originalRotation.x; }
        transform.rotation = Quaternion.Euler(rotation); // Assign new rotations with locks
    }
}
