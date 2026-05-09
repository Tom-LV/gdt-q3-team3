using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaterPuzzleManager : MonoBehaviour
{
    [Header("Ball and goal Settings")]
    public Transform puzzleBall;
    public Transform ballSpawnPoint;

    [Header("Timeline console")]
    [SerializeField] private TimelineConsole console;

    public UnityEvent OnPuzzleSolved;

    private Rigidbody ballRb;

    void Start()
    {
        if (puzzleBall != null)
        {
            ballRb = puzzleBall.GetComponent<Rigidbody>();
        }
        ResetBall();
    }

    public void CompletePuzzle()
    {
        Debug.Log("Puzzle Solved!");

        if (OnPuzzleSolved != null) OnPuzzleSolved.Invoke();
    }

    public void ResetBall()
    {
        if (ballRb != null)
        {
            ballRb.isKinematic = true; // Briefly stop physics
            puzzleBall.position = ballSpawnPoint.position;
            ballRb.isKinematic = false;
            ballRb.linearVelocity = Vector3.zero;
        }
    }
}