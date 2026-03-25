using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Player Reference")]
    public PlayerControls player;
    private CharacterController playerController;

    private Vector3 savedPosition;
    private Quaternion savedRotation;

    private PuzzleRoom activeRoom;

    private void Awake()
    {
        playerController = player.GetComponent<CharacterController>();
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
        if (player == null || player.IsShifting()) return;
        player.ShiftToPos(savedPosition, savedRotation);

        if (activeRoom != null)
        {
            activeRoom.ResetRoom();
        }
    }
}