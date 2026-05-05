using UnityEngine;

public class TimerDial : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("When the water turns ON (in seconds).")]
    public float startTime = 0f;
    [Tooltip("How long the water stays ON (in seconds).")]
    public float length = 3f;
    [Tooltip("How much time changes per button press.")]
    public float interval = 0.5f;

    [Header("Scene References")]
    public WaterPuzzleManager waterPuzzleManager;

    [Tooltip("Empty GameObject placed at the exact 0-second mark of the physical track.")]
    public Transform trackStart;
    [Tooltip("Empty GameObject placed at the exact maxTime mark of the physical track.")]
    public Transform trackEnd;

    private void Start()
    {
        UpdateVisuals();
    }

    public void MoveRight()
    {
        startTime += interval;

        // Clamp so the RIGHT edge of the block doesn't go past maxTime
        if (startTime + length > waterPuzzleManager.maxTime)
        {
            startTime = waterPuzzleManager.maxTime - length;
        }
        UpdateVisuals();
    }

    public void MoveLeft()
    {
        startTime -= interval;

        // Clamp so the LEFT edge doesn't go below 0
        if (startTime < 0)
        {
            startTime = 0;
        }
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (trackStart == null || trackEnd == null || waterPuzzleManager == null)
        {
            Debug.LogWarning("TimerDial is missing references!");
            return;
        }

        // Find the total physical distance between the start and end points
        float totalPhysicalLength = Vector3.Distance(trackStart.localPosition, trackEnd.localPosition);

        // Find what percentage of the total maxTime this block occupies
        float lengthPercentage = length / waterPuzzleManager.maxTime;
        float blockPhysicalLength = totalPhysicalLength * lengthPercentage;

        // Apply scale
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, blockPhysicalLength);

        // Standard Unity cubes pivot from the center. So we must find the time at the exact middle of the block.
        float centerTime = startTime + (length / 2f);
        float centerPercentage = centerTime / waterPuzzleManager.maxTime;

        // Smoothly interpolate between the start and end anchors using Local Position
        transform.localPosition = Vector3.Lerp(trackStart.localPosition, trackEnd.localPosition, centerPercentage);
    }
}