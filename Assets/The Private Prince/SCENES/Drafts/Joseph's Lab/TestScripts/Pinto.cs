using UnityEngine;

public class Pinto : IObject
{
    public GameObject target;
    public override void Interact()
    {
        Debug.Log($"Pinto.Interact() called on {gameObject.name}");
        Debug.Log($"Target is assigned: {target != null}");
        Debug.Log($"Target position: {target.transform.position}");

        if (target == null)
        {
            Debug.LogError("Pinto target NOT assigned");
            return;
        }

        TestSceneControlScript.TransitionPlayer(target.transform.position);
    }

}
