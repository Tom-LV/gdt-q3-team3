using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimelineConsole : MonoBehaviour
{
    [Header("Timeline Scrubber")]
    [Tooltip("The actual sliding scrubber object.")]
    public Transform timelineScrubber;
    [Tooltip("Empty GameObject placed at the exact 0-second mark of the track.")]
    public Transform trackStartAnchor;
    [Tooltip("Empty GameObject placed at the exact maxTime mark of the track.")]
    public Transform trackEndAnchor;

    [Header("Puzzle Elements")]
    [SerializeField] private List<TimelineEvent> timelineEvents;

    [SerializeField] private UnityEvent OnPlay;
    [SerializeField] private UnityEvent OnReset;

    [SerializeField] private float  maxTime = 20f ;
    private bool isPlaying = false;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetPuzzle();
    }

    public float GetMaxTime()
    {
        return maxTime;
    }

    public void Play()
    {
        ResetPuzzle();
        isPlaying = true;
        if (OnPlay != null) OnPlay.Invoke();
        Debug.Log("Puzzle Started!");
    }

    void FixedUpdate()
    {
        if (!isPlaying) return;

        timer += Time.fixedDeltaTime;

        // Calculate what percentage of the total time has passed (0.0 to 1.0)
        float timePercentage = Mathf.Clamp01(timer / maxTime);

        // Move the scrubber smoothly between the start and end anchors
        if (timelineScrubber != null && trackStartAnchor != null && trackEndAnchor != null)
        {
            Vector3 startPos = timelineScrubber.localPosition;
            startPos.z = trackStartAnchor.localPosition.z;
            Vector3 endPos = timelineScrubber.localPosition;
            endPos.z = trackEndAnchor.localPosition.z;
            timelineScrubber.localPosition = Vector3.Lerp(startPos, endPos, timePercentage);
        }

        // Update every event
        foreach (var timelineEvent in timelineEvents)
        {
            timelineEvent.EvaluateTime(timer);
        }

        // Stop the puzzle if we reach the end of the timeline
        if (timer >= maxTime)
        {
            isPlaying = false;
        }
    }

    public void ResetPuzzle()
    {
        isPlaying = false;
        timer = 0f;

        // Reset scrubber visually to the start anchor position
        if (timelineScrubber != null && trackStartAnchor != null)
        {
            Vector3 startPos = timelineScrubber.localPosition;
            startPos.z = trackStartAnchor.localPosition.z;
            timelineScrubber.localPosition = startPos;
        }

        // Turn off all events
        foreach (var timelineEvent in timelineEvents)
        {
            timelineEvent.ResetEvent();
        }
        OnReset.Invoke();
    }
}
