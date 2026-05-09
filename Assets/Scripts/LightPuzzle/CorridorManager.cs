using UnityEngine;

public class CorridorManager : MonoBehaviour
{
    [SerializeField] private PuzzleRoom room;

    public void OpenDoor()
    {
        GetCurrentDoorInstance()?.Open();
    }

    public void CloseDoor()
    {
        GetCurrentDoorInstance()?.Close();
    }

    private Door GetCurrentDoorInstance()
    {
        GameObject roomInstance = room.GetRoomInstance();
        return roomInstance.GetComponentInChildren<Door>();
    }
}
