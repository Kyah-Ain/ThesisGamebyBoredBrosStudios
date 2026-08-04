using UnityEngine;
using UnityEngine.Events;

public enum PuzzleGateAction
{
    DisableObject,
    EnableObject,
    PlayAnimation,
    InvokeEvent
}

public class PuzzleGate : MonoBehaviour
{
    [SerializeField] private PuzzleGateAction action;

    [Header("Disable / Enable")]
    [SerializeField] private GameObject targetObject;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTrigger = "Open";

    [Header("Custom Event")]
    [SerializeField] private UnityEvent onOpened;

    public void Open()
    {
        switch (action)
        {
            case PuzzleGateAction.DisableObject:

                if (targetObject == null)
                    targetObject = gameObject;

                targetObject.SetActive(false);

                break;

            case PuzzleGateAction.EnableObject:

                if (targetObject != null)
                    targetObject.SetActive(true);

                break;

            case PuzzleGateAction.PlayAnimation:

                if (animator != null)
                    animator.SetTrigger(animationTrigger);

                break;

            case PuzzleGateAction.InvokeEvent:

                onOpened?.Invoke();

                break;
        }
    }
}