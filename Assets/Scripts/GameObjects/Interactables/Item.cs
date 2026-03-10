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
        rb.useGravity = false;
        ignoredCol = player;
        Physics.IgnoreCollision(col, ignoredCol, true);
        rb.linearVelocity = Vector3.zero;
        isHeld = true;
    }
    public void SetPosition(Vector3 newPos, Vector3 camForward)
    {
        if (isHeld)
        {
            rb.MovePosition(newPos);
        }
    }
    public void Drop()
    {
        rb.useGravity = true;
        Physics.IgnoreCollision(col, ignoredCol, false);
        ignoredCol = null;
        isHeld = false;
    }
    public void Throw(Vector3 direction)
    {
        Drop();
        rb.AddForce(direction, ForceMode.Impulse);
    }
}
