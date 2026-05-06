using UnityEngine;

public class LavaPlatform : MonoBehaviour
{
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float lifeTime;

    [SerializeField]
    private float floorDetectionHeight;

    private float age = 0f;
    private BoxCollider platformCollider;

    private Transform objectOnPlatform;
    private Vector3 previousPosition; // Tracks platform movement

    void Start()
    {
        platformCollider = GetComponent<BoxCollider>();
        previousPosition = transform.position; // Set initial position
    }

    void FixedUpdate()
    {
        // 1. Move the platform (Note: changed to fixedDeltaTime for FixedUpdate)
        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;

        // 2. Calculate how much the platform moved this frame
        Vector3 deltaPosition = transform.position - previousPosition;

        // 3. Move the player by the exact same amount
        if (objectOnPlatform != null)
        {
            objectOnPlatform.position += deltaPosition;
        }

        // 4. Update the previous position for the next frame
        previousPosition = transform.position;

        // 5. Handle Lifetime
        age += Time.fixedDeltaTime;
        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Consider adding a tag check here, e.g., if(other.CompareTag("Player"))
        objectOnPlatform = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectOnPlatform == other.transform)
        {
            objectOnPlatform = null;
        }
    }
}