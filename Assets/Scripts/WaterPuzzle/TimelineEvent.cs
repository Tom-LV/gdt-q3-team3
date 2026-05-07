using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class TimelineEvent : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("When the event turns ON (in seconds).")]
    public float startTime = 0f;
    [Tooltip("How long the event stays ON (in seconds).")]
    public float length = 3f;
    [Tooltip("How much time changes per button press.")]
    public float interval = 0.5f;

    [Header("Track Layout")]
    [Tooltip("Which track row this sits on (0 is the main line, 1 is above, -1 is below).")]
    public int rowLevel = 0;
    [Tooltip("The vertical physical distance between each track row.")]
    public float rowSpacing = 0.5f;

    [Header("Events")]
    [Tooltip("Fires once when the timer reaches the Start Time.")]
    public UnityEvent OnEventStart;
    [Tooltip("Fires once when the timer passes (Start Time + Length).")]
    public UnityEvent OnEventEnd;

    [Header("UI & Scene References")]
    public WaterPuzzleManager waterPuzzleManager;
    [Tooltip("Empty GameObject placed at the exact 0-second mark of the physical track.")]
    public Transform trackStart;
    [Tooltip("Empty GameObject placed at the exact maxTime mark of the physical track.")]
    public Transform trackEnd;

    private bool isActive = false;

    private void Start()
    {
        UpdateVisuals();
        ResetEvent();
    }

    // INTERACTION LOGIC (Moving the block)
    public void MoveRight()
    {
        startTime += interval;

        // Clamp so the RIGHT edge of the block doesn't go past maxTime
        if (waterPuzzleManager != null && (startTime + length > waterPuzzleManager.maxTime))
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

    public void UpdateVisuals()
    {
        if (trackStart == null || trackEnd == null || waterPuzzleManager == null)
        {
            Debug.LogWarning($"TimelineEvent '{gameObject.name}' is missing UI references!");
            return;
        }

        // Calculate and set the physical SCALE
        float totalPhysicalLength = Vector3.Distance(trackStart.localPosition, trackEnd.localPosition);
        float lengthPercentage = length / waterPuzzleManager.maxTime;
        float blockPhysicalLength = totalPhysicalLength * lengthPercentage;

        // Apply scale
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, blockPhysicalLength);

        // Calculate and set the physical POSITION
        float centerTime = startTime + (length / 2f);
        float centerPercentage = centerTime / waterPuzzleManager.maxTime;

        Vector3 targetPos = Vector3.Lerp(trackStart.localPosition, trackEnd.localPosition, centerPercentage);
        targetPos.y += (rowLevel * rowSpacing);

        // Smoothly interpolate between the start and end anchors using Local Position
        transform.localPosition = targetPos;
    }

    // EXECUTION LOGIC (Running the puzzle)

    // Called every frame by the WaterPuzzleManager while the puzzle is playing
    public void EvaluateTime(float currentTime)
    {
        // Check if the current time falls inside our active window
        bool shouldBeActive = (currentTime >= startTime && currentTime <= (startTime + length));

        // State change: Turning ON
        if (shouldBeActive && !isActive)
        {
            isActive = true;
            if (OnEventStart != null) OnEventStart.Invoke();
        }
        // State change: Turning OFF
        else if (!shouldBeActive && isActive)
        {
            isActive = false;
            if (OnEventEnd != null) OnEventEnd.Invoke();
        }
    }

    public void ResetEvent()
    {
        // Force the event to turn off when the puzzle resets
        if (isActive)
        {
            isActive = false;
            if (OnEventEnd != null) OnEventEnd.Invoke();
        }
    }
}
