using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerControls player;

    [Header("Hold Settings")]
    public Transform holdPosition;
    [SerializeField] private float throwForceMultiplier = 12f;

    [Tooltip("How thick the item is. Used to prevent dropping items inside walls.")]
    [SerializeField] private float itemRadius = 0.2f;

    private bool canInteract = true;
    private InteractableItem lookingAtItem;
    private PickableItem currentHeldItem;

    private InputAction interactAction;
    // Removed dropAction since Interact handles it now!

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    public void SetInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public Transform GetPickupHandAndPickUpItem(PickableItem item)
    {
        if (currentHeldItem == null)
        {
            currentHeldItem = item;
            return holdPosition;
        }
        return null;
    }

    void Update()
    {
        if (!canInteract || PhoneController.isGamePaused)
        {
            ClearHoverState();
            return;
        }
        if (player.IsPushing())
        {
            if (interactAction.WasPressedThisFrame()) player.ClearPushState();
            return;
        }

        // --- RAYCAST & HOVER LOGIC ---
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange))
        {
            InteractableItem interactableItem = hit.collider.GetComponentInParent<InteractableItem>();

            if (interactableItem != null && interactableItem.IsInteractable())
            {
                if (lookingAtItem != interactableItem)
                {
                    ClearHoverState();
                    lookingAtItem = interactableItem;
                    lookingAtItem.OnHoverEnter(this);
                }
            }
            else
            {
                ClearHoverState();
            }
        }
        else
        {
            ClearHoverState();
        }

        // --- UNIFIED INPUT LOGIC ---
        if (lookingAtItem != null)
        {
            // 1. LOOKING AT SOMETHING: Interact with it (Pickup, Place in Slot, etc.)
            if (interactAction.WasPressedThisFrame())
            {
                lookingAtItem.OnInteract(this);
            }
            else if (interactAction.WasReleasedThisFrame())
            {
                lookingAtItem.StopInteract();
            }
        }
        else if (currentHeldItem != null)
        {
            // 2. LOOKING AT NOTHING & HOLDING AN ITEM: Drop or Throw it
            if (interactAction.WasPressedThisFrame())
            {
                DropItem(currentHeldItem, holdPosition);
            }
        }
    }

    private void ClearHoverState()
    {
        if (lookingAtItem != null)
        {
            lookingAtItem.StopInteract();
            lookingAtItem.OnHoverLeave(this);
            lookingAtItem = null;
        }
    }

    private void DropItem(PickableItem itemToDrop, Transform holdPos)
    {
        itemToDrop.transform.position = GetSafeDropPosition(holdPos);

        // If you are sprinting while pressing Interact, it throws the item instead!
        if (player.IsSprinting())
        {
            Vector3 force = cameraTransform.forward * throwForceMultiplier;
            force += Vector3.up * (throwForceMultiplier * 0.2f);

            itemToDrop.Throw(force);
        }
        else
        {
            itemToDrop.Drop();
        }

        ClearItem(itemToDrop);
    }

    public void ClearItem(PickableItem itemToClear)
    {
        // This ensures the player's hands are empty
        if (currentHeldItem == itemToClear)
        {
            currentHeldItem = null;
        }
    }

    private Vector3 GetSafeDropPosition(Transform holdPos)
    {
        Vector3 origin = cameraTransform.position;
        Vector3 target = holdPos.position;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (Physics.SphereCast(origin, itemRadius, direction.normalized, out RaycastHit hit, distance))
        {
            return hit.point + (hit.normal * itemRadius);
        }

        return target;
    }

    //---------------------
    // special interactions

    public void StartPushInteraction(PushableItem pushable)
    {
        player.SetPushObject(pushable);
    }

    public PickableItem GetKeyItem(string type)
    {
        if (currentHeldItem?.HasType(type) == true) return currentHeldItem;
        return null;
    }

    public PickableItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public Vector3 GetCameraForward()
    {
        if (cameraTransform != null)
        {
            return cameraTransform.forward;
        }
        return transform.forward;
    }
}