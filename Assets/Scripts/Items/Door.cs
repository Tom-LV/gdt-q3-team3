using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] protected UnityEvent m_OnOpen;
    [SerializeField] protected UnityEvent m_OnClose;

    [Header("Door Settings")]
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 relativeDoorPos = new Vector3(0, 4, 0);
    [SerializeField] private float openTime = 4f;
    [SerializeField] private bool startOpen = false;

    private Vector3 closedPos;
    private Vector3 openPos;

    private bool targetStateIsOpen = false;

    // Tracks where the door currently is
    private float transitionProgress = 0f;

    // Prevents the UnityEvents from firing every single frame once the door stops moving
    private bool eventFired = false;

    void Awake()
    {
        // Calculate start and end positions
        closedPos = door.localPosition;
        openPos = closedPos + relativeDoorPos;
        if(startOpen) Open();
    }

    public void Open()
    {
        if (!targetStateIsOpen)
        {
            targetStateIsOpen = true;
            eventFired = false; // Reset the flag so the Open event can fire when it reaches the top
        }
    }

    public void Close()
    {
        if (targetStateIsOpen)
        {
            targetStateIsOpen = false;
            eventFired = false; // Reset the flag so the Close event can fire when it reaches the bottom
        }
    }

    private void Update()
    {
        // Adjust the progress depending on whether we want to be open or closed
        if (targetStateIsOpen && transitionProgress < 1f)
        {
            // Move towards 1
            transitionProgress += Time.deltaTime / openTime;
        }
        else if (!targetStateIsOpen && transitionProgress > 0f)
        {
            // Move towards 0
            transitionProgress -= Time.deltaTime / openTime;
        }

        // Clamp the progress so it never goes below 0 or above 1
        transitionProgress = Mathf.Clamp01(transitionProgress);

        // Move the door
        door.localPosition = Vector3.Lerp(closedPos, openPos, transitionProgress);

        // Check if we have arrived at our destination and fire the correct event ONCE
        if (targetStateIsOpen && transitionProgress == 1f && !eventFired)
        {
            eventFired = true;
            if (m_OnOpen != null) m_OnOpen.Invoke();
        }
        else if (!targetStateIsOpen && transitionProgress == 0f && !eventFired)
        {
            eventFired = true;
            if (m_OnClose != null) m_OnClose.Invoke();
        }
    }
}