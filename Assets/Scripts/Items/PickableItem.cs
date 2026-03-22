using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickableItem : InteractableItem
{
    [SerializeField]
    UnityEvent m_OnPickup;

    [SerializeField]
    UnityEvent m_OnDrop;

    private Rigidbody rb;
    private Collider col;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public override void OnInteract(PlayerInteract player)
    {
        if (m_OnInteract != null) m_OnInteract.Invoke();

        Pickup(player);
    }

    private void Pickup(PlayerInteract player)
    {
        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(player.holdPosition);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        player.SetHeldItem(this);

        OnHoverLeave();

        if (m_OnPickup != null) m_OnPickup.Invoke();
    }

    public void Drop()
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;

        if (m_OnDrop != null) m_OnDrop.Invoke();
    }

    public void Throw(Vector3 throwForce)
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        col.enabled = true;

        rb.AddForce(throwForce, ForceMode.Impulse);

        if (m_OnDrop != null) m_OnDrop.Invoke();
    }
}