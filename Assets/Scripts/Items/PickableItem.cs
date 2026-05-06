using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickableItem : InteractableItem
{
    [SerializeField] protected UnityEvent m_OnPickup;
    [SerializeField] protected UnityEvent m_OnDrop;
    [SerializeField] protected UnityEvent m_OnUse;
    [SerializeField] private int keyID;

    private Rigidbody rb;
    private Collider col;


    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public override bool OffCooldown() // now the cooldown is not for "Interacting" (picking up) but "Using" the item, soooo
    {
        return true;
    }
    public override void OnInteract(PlayerInteract player)
    {
        if (m_OnInteract != null) m_OnInteract.Invoke();

        Pickup(player.GetPickupHandAndPickUpItem(this));
    }

    private void Pickup(Transform holdPosition)
    {
        if(holdPosition == null) return;

        rb.isKinematic = true;
        col.enabled = false;

        transform.SetParent(holdPosition);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        OnHoverLeave();

        if (m_OnPickup != null) m_OnPickup.Invoke();
    }

    public void Use()
    {
        if(Time.time - lastUseTime < cooldown) return;
        lastUseTime = Time.time;

        if (m_OnUse != null) m_OnUse.Invoke();
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

    public bool HasKeyID(int other)
    {
        return other == keyID;
    }
}