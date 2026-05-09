using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoverBehavior : MonoBehaviour
{
    [Tooltip("The local position to move to when active.")]
    public Vector3 targetLocalPosition;
    public float moveSpeed = 2f;

    private Vector3 startLocalPosition;
    private bool isMovingToTarget = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        startLocalPosition = transform.localPosition;
    }

    public void ResetState()
    {
        transform.localPosition = startLocalPosition;
        SetActive(false);
    }

    public void SetActive(bool state)
    {
        isMovingToTarget = state;
    }

    void FixedUpdate()
    {
        Vector3 destination = isMovingToTarget ? targetLocalPosition : startLocalPosition;

        if (transform.localPosition != destination)
        {
            // 1. Calculate the new local position
            Vector3 newLocalPos = Vector3.MoveTowards(transform.localPosition, destination, moveSpeed * Time.fixedDeltaTime);

            // 2. Convert that local position into World Space
            Vector3 newWorldPos = transform.parent != null ? transform.parent.TransformPoint(newLocalPos) : newLocalPos;

            // 3. Tell the physics engine to physically sweep the object to the new world position
            rb.MovePosition(newWorldPos);
        }
    }
}