using UnityEngine;
using UnityEngine.Events;

public class PuzzleRoom : MonoBehaviour
{
    [Header("Room Setup")]
    [Tooltip("Drag room content prefab here")]
    [SerializeField] private GameObject roomPrefab;
    private GameObject currentRoomInstance;

    [SerializeField]
    private UnityEvent m_OnReload;

    void Start()
    {
        SpawnFreshRoom();
    }

    public void ResetRoom()
    {
        // Destroy the room
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        if (m_OnReload != null) m_OnReload.Invoke();

        // Spawn a new room
        SpawnFreshRoom();
    }

    private void SpawnFreshRoom()
    {
        if (roomPrefab == null) return;
        currentRoomInstance = Instantiate(roomPrefab, transform.position, transform.rotation, this.transform);
    }
}