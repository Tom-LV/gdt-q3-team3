using UnityEngine;

public class PuzzleRoom : MonoBehaviour
{
    [Header("Room Setup")]
    [Tooltip("Drag room content prefab here")]
    [SerializeField] private GameObject roomPrefab;
    private GameObject currentRoomInstance;

    void Start()
    {
        SpawnFreshRoom();
    }
    public GameObject GetRoomInstance()
    {
        return currentRoomInstance;
    }

    public void ResetRoom()
    {
        // Destroy the room
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        // Spawn a new room
        SpawnFreshRoom();
    }

    private void SpawnFreshRoom()
    {
        if (roomPrefab == null) return;
        currentRoomInstance = Instantiate(roomPrefab, transform.position, transform.rotation, this.transform);
    }
}