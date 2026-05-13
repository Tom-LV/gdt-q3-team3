using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickableItem : InteractableItem
{
    [SerializeField] protected UnityEvent m_OnPickup;
    [SerializeField] protected UnityEvent m_OnDrop;
    [SerializeField] protected UnityEvent m_OnUse;
    [SerializeField] private string type;

    private Rigidbody rb;
    private Collider[] allColliders;

    private Transform currentHoldPosition;
    private bool isHeld = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        allColliders = GetComponentsInChildren<Collider>(true);
    }

    public override bool OffCooldown()
    {
        return true;
    }

    public override void OnInteract(PlayerInteract player)
    {
        if (m_OnInteract != null) m_OnInteract.Invoke();
        Pickup(player.GetPickupHandAndPickUpItem(this));
    }

    // Made public so the ItemSlot can give the item back to the player
    public void Pickup(Transform holdPosition)
    {
        if (holdPosition == null) return;

        rb.isKinematic = true;
        foreach (Collider col in allColliders)
        {
            if (col != null) col.enabled = false;
        }

        currentHoldPosition = holdPosition;
        isHeld = true;

        OnHoverLeave();

        if (m_OnPickup != null) m_OnPickup.Invoke();
    }

    // --- NEW: Freezes the item onto a slot ---
    public void PlaceInSlot(Transform slotPosition, Quaternion slotRotation)
    {
        isHeld = false;
        currentHoldPosition = null;

        rb.isKinematic = true;
        foreach (Collider col in allColliders)
        {
            if (col != null) col.enabled = false;
        }

        transform.position = slotPosition.position;
        transform.rotation = slotRotation;
    }
    // -----------------------------------------

    public void Use()
    {
        if (Time.time - lastUseTime < cooldown) return;
        lastUseTime = Time.time;
        if (m_OnUse != null) m_OnUse.Invoke();
    }

    public void Drop()
    {
        isHeld = false;
        currentHoldPosition = null;
        rb.isKinematic = false;
        foreach (Collider col in allColliders)
        {
            if (col != null) col.enabled = true;
        }
        if (m_OnDrop != null) m_OnDrop.Invoke();
    }

    public void Throw(Vector3 throwForce)
    {
        isHeld = false;
        currentHoldPosition = null;
        rb.isKinematic = false;
        foreach (Collider col in allColliders)
        {
            if (col != null) col.enabled = true;
        }
        rb.AddForce(throwForce, ForceMode.Impulse);
        if (m_OnDrop != null) m_OnDrop.Invoke();
    }

    public bool HasType(string other)
    {
        return other == type;
    }

    private void LateUpdate()
    {
        if (isHeld && currentHoldPosition != null)
        {
            transform.position = currentHoldPosition.position;
            transform.rotation = currentHoldPosition.rotation;
        }
    }

    public bool IsHeld()
    {
        return isHeld;
    }
}