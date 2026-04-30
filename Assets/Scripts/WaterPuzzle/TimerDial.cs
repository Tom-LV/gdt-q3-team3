using UnityEngine;

public class TimerDial : MonoBehaviour
{
    [Tooltip("The current time setting on this dial (in seconds).")]
    public int currentTime = 0;

    [Tooltip("The maximum time before the dial loops back to 0.")]
    public int maxTime = 10;

    [Tooltip("The visual part of the dial to rotate.")]
    public Transform visualDial;

    // Call this from your PlayerInteract script when the player clicks the dial
    public void Interact()
    {
        currentTime++;
        if (currentTime > maxTime)
        {
            currentTime = 0;
        }

        // Rotate the visual representation (assumes Y axis rotation, adjust if needed)
        if (visualDial != null)
        {
            float rotationAngle = (360f / (maxTime + 1)) * currentTime;
            visualDial.localRotation = Quaternion.Euler(0, rotationAngle, 0);
        }
    }
}
