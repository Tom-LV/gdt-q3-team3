using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotatorBehavior : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;

    private bool isRotating = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Crucial: tells Unity this physical object is driven by a script, not gravity
    }

    public void SetActive(bool state)
    {
        isRotating = state;
    }

    // FixedUpdate runs at a locked time step (default 50 times per second), making it deterministic
    void FixedUpdate()
    {
        if (isRotating)
        {
            // Calculate the rotation amount for this specific physics frame
            Quaternion deltaRotation = Quaternion.Euler(rotationAxis * rotationSpeed * Time.fixedDeltaTime);

            // Tell the physics engine to smoothly sweep to the new rotation
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    public void ResetRotator()
    {
        isRotating = false;
        transform.rotation = Quaternion.identity;
    }
}