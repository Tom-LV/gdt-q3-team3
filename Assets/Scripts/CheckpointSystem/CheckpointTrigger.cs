using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CheckpointTrigger : MonoBehaviour
{
    [Tooltip("Should this checkpoint only work once per life?")]
    public bool triggerOnlyOnce = true;

    [Tooltip("Drag the PuzzleRoom script here")]
    public PuzzleRoom linkedRoom;

    [Tooltip("Respawn transform")]
    public Transform respawnTransform;

    private bool hasTriggered = false;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce) return;

        if (other.CompareTag("Player"))
        {
            // Tell the manager to save this location and link the puzzle room

            CheckpointManager.Instance.SaveCheckpoint(respawnTransform == null ? this.transform : respawnTransform, linkedRoom);
            hasTriggered = true;
        }
    }
}