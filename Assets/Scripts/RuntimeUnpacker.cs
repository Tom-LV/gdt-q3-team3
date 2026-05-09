using UnityEngine;

public class RuntimeUnpacker : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform phone;
    [SerializeField] private Transform checkpointManager;
    [SerializeField] private Transform cutsceneManager;

    private void Awake()
    {
        Detach(player);
        Detach(phone);
        Detach(checkpointManager);
        Detach(cutsceneManager);
        Destroy(gameObject);
    }

    private void Detach(Transform target)
    {
        if (target == null) return;
        target.SetParent(null, true);
    }
}