using System.Collections.Generic;
using UnityEngine;

public class WaterPuzzleManager : MonoBehaviour
{
    [Header("Ball Settings")]
    public Transform puzzleBall;
    public Transform ballSpawnPoint;

    [Header("Puzzle Elements")]
    public List<WaterCurrent> waterCurrents;

    private bool isPlaying = false;
    private float timer = 0f;
    private Rigidbody ballRb;

    void Start()
    {
        if (puzzleBall != null)
        {
            ballRb = puzzleBall.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        // Tell every pipe to check if it should be on or off based on the current time
        foreach (var current in waterCurrents)
        {
            current.EvaluateTime(timer);
        }
    }

    // Call this from your player interact script when the Master Play Button is clicked
    public void PressPlayButton()
    {
        ResetPuzzle();
        isPlaying = true;
        Debug.Log("Puzzle Started!");
    }

    public void ResetPuzzle()
    {
        isPlaying = false;
        timer = 0f;

        // Reset Ball physics and position
        if (ballRb != null)
        {
            ballRb.isKinematic = true; // Briefly stop physics
            puzzleBall.position = ballSpawnPoint.position;
            ballRb.isKinematic = false;
            ballRb.linearVelocity = Vector3.zero;
        }

        // Turn off all currents
        foreach (var current in waterCurrents)
        {
            current.SetFlowActive(false);
        }
    }

    public void FailRun()
    {
        Debug.Log("Ball sank in a vortex!");
        // Optional: Play a splash sound or sink animation here before resetting
        ResetPuzzle();
    }
}
