using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementManager))]
public class PuzzleMovementBlocker : MonoBehaviour
{
    private MovementManager movementManager;
    private CharacterController characterController;

    private void Awake()
    {
        movementManager = GetComponent<MovementManager>();
        characterController = GetComponent<CharacterController>();
        if (movementManager == null)
            Debug.LogError("PuzzleMovementBlocker requires a MovementManager component on the same GameObject.");
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator Start()
    {
        if (PuzzleManager.Instance == null)
        {
            yield return new WaitUntil(() => PuzzleManager.Instance != null);
            TrySubscribe();
        }
    }

    private void TrySubscribe()
    {
        if (PuzzleManager.Instance == null) return;

        PuzzleManager.Instance.OnPuzzleStarted -= OnPuzzleStarted;
        PuzzleManager.Instance.OnPuzzleEnded -= OnPuzzleEnded;
        PuzzleManager.Instance.OnPuzzleStarted += OnPuzzleStarted;
        PuzzleManager.Instance.OnPuzzleEnded += OnPuzzleEnded;
    }

    private void Unsubscribe()
    {
        if (PuzzleManager.Instance == null) return;
        PuzzleManager.Instance.OnPuzzleStarted -= OnPuzzleStarted;
        PuzzleManager.Instance.OnPuzzleEnded -= OnPuzzleEnded;
    }

    private void Update()
    {
        var pm = PuzzleManager.Instance;
        if (pm == null) return;

        if (pm.State == PuzzleState.InProgress)
        {
            if (movementManager != null && movementManager.canMove == true)
            {
                movementManager.canMove = false;
                ZeroOutMovement();
            }
        }
        else
        {
            if (movementManager != null && movementManager.canMove == false)
            {
                movementManager.canMove = true;
            }
            return;
        }
    }

    private void OnPuzzleStarted(PuzzleBase puzzle)
    {
        if (movementManager != null)
        {
            movementManager.canMove = false;
            ZeroOutMovement();
        }
    }

    private void OnPuzzleEnded(PuzzleBase puzzle, PuzzleResult result)
    {
        if (movementManager != null)
        {
            movementManager.canMove = true;
        }
    }

    private void ZeroOutMovement()
    {
        movementManager.CalculateMovement(Vector2.zero, false);
        movementManager.CalculateJump(false);

        if (characterController != null)
        {
            movementManager.ApplyMovement(characterController);
        }
    }
}
