using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Player Reference")]
    public CharacterController playerController;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private PuzzleRoom activeRoom;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (playerController != null)
        {
            savedPosition = playerController.transform.position;
            savedRotation = playerController.transform.rotation;
        }
    }

    public void SaveCheckpoint(Transform checkpointNode, PuzzleRoom roomToLink)
    {
        savedPosition = checkpointNode.position;
        savedRotation = checkpointNode.rotation;
        activeRoom = roomToLink;

        Debug.Log("Checkpoint saved");
    }

    public void ReloadCheckpoint()
    {
        if (playerController == null) return;

        playerController.enabled = false;
        playerController.transform.position = savedPosition;
        playerController.transform.rotation = savedRotation;
        playerController.enabled = true;

        if (activeRoom != null)
        {
            activeRoom.ResetRoom();
        }

        Debug.Log("Player respawned and room reset");
    }
}