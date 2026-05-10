using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LavaPlatform : MonoBehaviour
{
    private enum PlatformState { Rising, Moving, Sinking }
    private PlatformState currentState = PlatformState.Rising;

    [Header("Platform Settings")]
    [Tooltip("How fast the platform travels physically along the curve (Meters per Second).")]
    public float moveSpeed = 4f;
    public float verticalSpeed = 2f;
    public float sinkDepth = 2f;

    private Spline myPath;

    private Transform objectOnPlatform;
    private CharacterController playerCC;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private Vector3 currentVelocity;

    private float currentDistance = 0f;

    public void Initialize(Spline path)
    {
        myPath = path;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Vector3 startPos = myPath.GetPoint(0f);
        Vector3 hiddenSpawnPos = startPos - (Vector3.up * sinkDepth);
        transform.position = hiddenSpawnPos;
        rb.position = hiddenSpawnPos;
        previousPosition = transform.position;

        currentState = PlatformState.Rising;
    }

    void FixedUpdate()
    {
        if (myPath == null) return;

        Vector3 nextPos = rb.position;

        Vector3 startPos = myPath.GetPoint(0f);
        Vector3 endPos = myPath.GetPoint(1f);

        switch (currentState)
        {
            case PlatformState.Rising:
                nextPos = Vector3.MoveTowards(rb.position, startPos, verticalSpeed * Time.fixedDeltaTime);
                if (Vector3.Distance(rb.position, startPos) < 0.01f)
                    currentState = PlatformState.Moving;
                break;

            case PlatformState.Moving:
                // Move forward by constant physical speed
                currentDistance += moveSpeed * Time.fixedDeltaTime;

                // Check if we hit the total measured length of the spline
                if (currentDistance >= myPath.TotalLength)
                {
                    currentDistance = myPath.TotalLength;
                    currentState = PlatformState.Sinking;
                }

                // Get the position using our new Lookup Table method!
                nextPos = myPath.GetPointAtDistance(currentDistance);
                break;

            case PlatformState.Sinking:
                Vector3 sinkTarget = endPos - (Vector3.up * sinkDepth);
                nextPos = Vector3.MoveTowards(rb.position, sinkTarget, verticalSpeed * Time.fixedDeltaTime);
                if (Vector3.Distance(rb.position, sinkTarget) < 0.01f)
                    Destroy(gameObject);
                break;
        }

        Vector3 deltaPosition = nextPos - previousPosition;
        currentVelocity = deltaPosition / Time.fixedDeltaTime;

        rb.MovePosition(nextPos);

        if (objectOnPlatform != null)
        {
            if (playerCC != null) playerCC.Move(deltaPosition);
            else objectOnPlatform.position += deltaPosition;
        }

        previousPosition = nextPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectOnPlatform = other.transform;
            playerCC = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectOnPlatform == other.transform)
        {
            if (other.CompareTag("Player"))
            {
                PlayerControls player = other.GetComponent<PlayerControls>();
                if (player != null) player.AddMomentum(currentVelocity);
            }

            objectOnPlatform = null;
            playerCC = null;
        }
    }
}