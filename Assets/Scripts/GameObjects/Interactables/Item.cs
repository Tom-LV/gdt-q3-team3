using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private Vector3 heldRotation;
    private bool isHeld = false;
    private Rigidbody rb;
    private Collider col;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void PickUp()
    {
        rb.useGravity = false;
        col.enabled = false;
        rb.linearVelocity = Vector3.zero;
        isHeld = true;
    }
    public void SetPosition(Vector3 newPos, Vector3 camForward)
    {
        if (isHeld)
        {
            transform.position = newPos;
        }
    }
    public void Drop()
    {
        rb.useGravity = true;
        col.enabled = true;
        isHeld = false;
    }
    public void Throw(Vector3 direction)
    {
        Drop();
        rb.AddForce(direction, ForceMode.Impulse);
    }
}
