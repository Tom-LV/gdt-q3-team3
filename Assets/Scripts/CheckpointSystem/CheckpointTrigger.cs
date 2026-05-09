using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class CheckpointTrigger : MonoBehaviour
{
    [Tooltip("Should this checkpoint only work once per life?")]
    public bool triggerOnlyOnce = true;

    [Tooltip("Drag the PuzzleRoom script here")]
    public PuzzleRoom linkedRoom;

    [Tooltip("Respawn transform")]
    public Transform respawnTransform;

    [SerializeField]
    private UnityEvent m_OnCheckpointActivated;

    private bool hasTriggered = true;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnlyOnce) return;

        if (other.CompareTag("Player"))
        {
            if (m_OnCheckpointActivated != null) m_OnCheckpointActivated.Invoke();

            CheckpointManager.Instance.SaveCheckpoint(respawnTransform == null ? this.transform : respawnTransform, linkedRoom);
            hasTriggered = true;
        }
    }
}