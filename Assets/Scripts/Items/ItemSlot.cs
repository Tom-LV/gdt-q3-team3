using UnityEngine;
using UnityEngine.Events;

public class ItemSlot : InteractableItem
{
    [Header("Slot Settings")]
    [Tooltip("List of items and their ghosts accepted by this slot.")]
    [SerializeField] private GhostMapping[] acceptedGhosts;

    [Tooltip("Where the item will snap to. If left empty, it snaps to this object's center.")]
    [SerializeField] private Transform snapPosition;

    [Tooltip("Can the player pick the item back up after placing it?")]
    [SerializeField] private bool canRemove = true;

    [Header("Slot Events")]
    [SerializeField] private UnityEvent m_OnPlace;
    [SerializeField] private UnityEvent m_OnRemove;

    private PickableItem currentItem;
    private GameObject currentActiveGhost;
    private PlayerInteract hoveringPlayer;

    protected override void Start()
    {
        base.Start();
        // Ensure all ghosts are hidden at start
        if (acceptedGhosts != null)
        {
            foreach (var mapping in acceptedGhosts)
            {
                if (mapping.ghostVisual != null) mapping.ghostVisual.SetActive(false);
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        // If a ghost is visible, rotate it dynamically based on where the player is looking
        if (currentActiveGhost != null && hoveringPlayer != null)
        {
            currentActiveGhost.transform.rotation = GetPlayerFacingRotation(hoveringPlayer);
        }
    }

    // Calculates the closest 90-degree angle based on the camera
    private Quaternion GetPlayerFacingRotation(PlayerInteract player)
    {
        Vector3 playerForward = player.GetCameraForward();
        playerForward.y = 0; // Keep it flat on the floor

        if (playerForward.sqrMagnitude > 0.001f)
        {
            float snapAngle = Mathf.Round(Vector3.SignedAngle(Vector3.forward, playerForward.normalized, Vector3.up) / 90f) * 90f;
            return Quaternion.Euler(0, snapAngle, 0);
        }
        return Quaternion.identity;
    }

    // Helper: Finds the matching ghost for a held item
    private GameObject GetGhostForItem(PickableItem item)
    {
        if (item == null || acceptedGhosts == null) return null;
        foreach (var mapping in acceptedGhosts)
        {
            if (item.HasType(mapping.type)) return mapping.ghostVisual;
        }
        return null;
    }

    public override void OnHoverEnter(PlayerInteract player = null)
    {
        base.OnHoverEnter(player);
        hoveringPlayer = player;

        if (currentItem == null && player != null)
        {
            // 1. SLOT IS EMPTY: Show the ghost
            PickableItem heldItem = player.GetCurrentHeldItem();
            currentActiveGhost = GetGhostForItem(heldItem);

            if (currentActiveGhost != null)
            {
                currentActiveGhost.SetActive(true);
            }
        }
        else if (currentItem != null)
        {
            // 2. SLOT IS FULL: Manually outline the placed item!
            currentItem.OnHoverEnter(player);
        }
    }

    public override void OnHoverLeave(PlayerInteract player = null)
    {
        base.OnHoverLeave(player);
        hoveringPlayer = null;

        // Hide ghost
        if (currentActiveGhost != null)
        {
            currentActiveGhost.SetActive(false);
            currentActiveGhost = null;
        }

        // Remove outline from the placed item
        if (currentItem != null)
        {
            currentItem.OnHoverLeave(player);
        }
    }

    public override void OnInteract(PlayerInteract player)
    {
        base.OnInteract(player);

        if (currentItem == null)
        {
            // 1. SLOT IS EMPTY - Try to place
            PickableItem heldItem = player.GetCurrentHeldItem();

            if (GetGhostForItem(heldItem) != null)
            {
                player.ClearItem(heldItem);

                Transform targetSnap = snapPosition != null ? snapPosition : transform;
                Quaternion targetRotation = GetPlayerFacingRotation(player);

                heldItem.PlaceInSlot(targetSnap, targetRotation);
                currentItem = heldItem;

                if (currentActiveGhost != null)
                {
                    currentActiveGhost.SetActive(false);
                    currentActiveGhost = null;
                }

                if (m_OnPlace != null) m_OnPlace.Invoke();
                if (!canRemove) SetInteractable(false);

                // --- THE FIX ---
                // Refresh visuals immediately so the placed item gets its outline
                OnHoverEnter(player);
            }
        }
        else
        {
            // 2. SLOT IS FULL - Try to pick up
            if (canRemove)
            {
                Transform hand = player.GetPickupHandAndPickUpItem(currentItem);
                if (hand != null)
                {
                    currentItem.Pickup(hand);
                    currentItem = null;

                    if (m_OnRemove != null) m_OnRemove.Invoke();

                    // --- THE FIX ---
                    // Refresh visuals immediately so the ghost appears for the newly held item
                    OnHoverEnter(player);
                }
            }
        }
    }
}