using UnityEngine;

public class MoverBehavior : MonoBehaviour
{
    [Tooltip("The local position to move to when active.")]
    public Vector3 targetLocalPosition;
    public float moveSpeed = 2f;

    private Vector3 startLocalPosition;
    private bool isMovingToTarget = false;

    void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    // The TimelineEvent will call this
    public void SetActive(bool state)
    {
        isMovingToTarget = state;
    }

    void Update()
    {
        Vector3 destination = isMovingToTarget ? targetLocalPosition : startLocalPosition;

        // Smoothly move towards the destination
        if (transform.localPosition != destination)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination, moveSpeed * Time.deltaTime);
        }
    }
}