using System.Collections.Generic;
using UnityEngine;

public class WaterPuzzleManager : MonoBehaviour
{
    [Header("Timeline Scrubber")]
    [Tooltip("The actual sliding scrubber object.")]
    public Transform timelineScrubber;
    [Tooltip("Empty GameObject placed at the exact 0-second mark of the track.")]
    public Transform trackStartAnchor;
    [Tooltip("Empty GameObject placed at the exact maxTime mark of the track.")]
    public Transform trackEndAnchor;

    [Header("Ball Settings")]
    public Transform puzzleBall;
    public Transform ballSpawnPoint;

    [Header("Puzzle Elements")]
    public List<WaterCurrent> waterCurrents;

    public float maxTime = 20f;

    private bool isPlaying = false;
    private float timer = 0f;
    private Rigidbody ballRb;

    void Start()
    {
        if (puzzleBall != null)
        {
            ballRb = puzzleBall.GetComponent<Rigidbody>();
        }
        ResetPuzzle();
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;

        // --- SCRUBBER MOVEMENT ---
        // Calculate what percentage of the total time has passed (0.0 to 1.0)
        float timePercentage = Mathf.Clamp01(timer / maxTime);

        // Move the scrubber smoothly between the start and end anchors
        if (timelineScrubber != null && trackStartAnchor != null && trackEndAnchor != null)
        {
            timelineScrubber.position = Vector3.Lerp(trackStartAnchor.position, trackEndAnchor.position, timePercentage);
        }

        // Tell every pipe to check if it should be on or off based on the current time
        foreach (var current in waterCurrents)
        {
            current.EvaluateTime(timer);
        }

        // Stop the puzzle if we reach the end of the timeline
        if (timer >= maxTime)
        {
            ResetPuzzle();
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

        // Reset scrubber visually to the start anchor position
        if (timelineScrubber != null && trackStartAnchor != null)
        {
            timelineScrubber.position = trackStartAnchor.position;
        }

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