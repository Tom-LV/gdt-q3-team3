using Bitgem.VFX.StylisedWater;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterPuzzleGoal : MonoBehaviour
{
    [Tooltip("The tag of your puzzle ball.")]
    public string ballTag = "PuzzleBall";

    [Tooltip("Reference to the main puzzle manager.")]
    public WaterPuzzleManager manager;

    [Tooltip("Where the ball should snap to when it enters. If null, uses this object's position.")]
    public Transform snapPosition;

    private bool isSolved = false;

    private void OnTriggerEnter(Collider other)
    {
        // Don't trigger multiple times
        if (isSolved) return;

        if (other.CompareTag(ballTag))
        {
            isSolved = true;

            // Stop the ball's physics so it doesn't bounce around
            Rigidbody rb = other.attachedRigidbody;
            other.GetComponent<WateverVolumeFloater>().enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Snap to the exact correct position
            Vector3 targetPos = snapPosition != null ? snapPosition.position : transform.position;
            other.transform.position = targetPos;

            // Tell the manager we won!
            if (manager != null)
            {
                manager.CompletePuzzle();
            }
        }
    }

    // Called by the manager if the player resets the puzzle to try again
    public void ResetGoal()
    {
        isSolved = false;
    }
}