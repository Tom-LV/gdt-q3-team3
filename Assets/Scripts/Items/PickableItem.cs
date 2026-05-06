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

    // Track the hold position and whether the item is currently held
    private Transform currentHoldPosition;
    private bool isHeld = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
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

    private void Pickup(Transform holdPosition)
    {
        if (holdPosition == null) return;

        rb.isKinematic = true;
        col.enabled = false;

        // Instead of setting parent, we store the target and flag it as held
        currentHoldPosition = holdPosition;
        isHeld = true;

        OnHoverLeave();

        if (m_OnPickup != null) m_OnPickup.Invoke();
    }

    public void Use()
    {
        if (Time.time - lastUseTime < cooldown) return;
        lastUseTime = Time.time;

        if (m_OnUse != null) m_OnUse.Invoke();
    }

    public void Drop()
    {
        // Clear the held state instead of setting parent to null
        isHeld = false;
        currentHoldPosition = null;

        rb.isKinematic = false;
        col.enabled = true;

        if (m_OnDrop != null) m_OnDrop.Invoke();
    }

    public void Throw(Vector3 throwForce)
    {
        // Clear the held state instead of setting parent to null
        isHeld = false;
        currentHoldPosition = null;

        rb.isKinematic = false;
        col.enabled = true;

        rb.AddForce(throwForce, ForceMode.Impulse);

        if (m_OnDrop != null) m_OnDrop.Invoke();
    }

    public bool HasKeyID(int other)
    {
        return other == keyID;
    }

    // LateUpdate runs after Update. This ensures the player's movement and animations 
    // are fully calculated before we snap the item to the hand, preventing jitter.
    private void LateUpdate()
    {
        if (isHeld && currentHoldPosition != null)
        {
            transform.position = currentHoldPosition.position;
            transform.rotation = currentHoldPosition.rotation;
        }
    }
}