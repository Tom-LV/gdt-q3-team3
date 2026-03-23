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

    void Start()
    {
        platformCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        Vector3 movementThisFrame = moveDirection * moveSpeed * Time.deltaTime;
        transform.position += movementThisFrame;

        if (platformCollider != null)
        {
            Vector3 halfExtents = platformCollider.bounds.extents;
            halfExtents.y = floorDetectionHeight;

            // Center the radar perfectly on top of the platform
            Vector3 boxCenter = platformCollider.bounds.center;
            boxCenter.y += platformCollider.bounds.extents.y + floorDetectionHeight;

            Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, transform.rotation);

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    CharacterController cc = hit.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        Vector3 push = movementThisFrame;

                        // Keep a tiny bit of gravity so the player knows it is grounded
                        push.y = -0.01f;

                        cc.Move(push);
                    }
                }
            }
        }

        age += Time.deltaTime;
        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    // Visual
    private void OnDrawGizmos()
    {
        if (platformCollider == null) platformCollider = GetComponent<BoxCollider>();
        if (platformCollider == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Transparent green

        Vector3 halfExtents = platformCollider.bounds.extents;
        halfExtents.y = floorDetectionHeight;

        Vector3 boxCenter = platformCollider.bounds.center;
        boxCenter.y += platformCollider.bounds.extents.y + floorDetectionHeight;

        Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(boxCenter, halfExtents * 2);
    }
}