using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private Vector3 heldRotation;
    private bool isHeld = false;
    private Rigidbody rb;
    private Collider col;
    private Collider ignoredCol;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void PickUp(Collider player)
    {
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        ignoredCol = player;
        Physics.IgnoreCollision(col, ignoredCol, true);
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
        rb.isKinematic = false;
        Physics.IgnoreCollision(col, ignoredCol, false);
        isHeld = false;
    }
    public void Throw(Vector3 direction)
    {
        Drop();
        rb.AddForce(direction, ForceMode.Impulse);
    }
}
