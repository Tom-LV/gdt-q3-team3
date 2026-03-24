using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerControls player;

    [Header("Hold Settings")]
    public Transform holdPositionR;
    public Transform holdPositionL;
    [SerializeField] private float throwForceMultiplier = 12f;

    [Tooltip("How thick the item is. Used to prevent dropping items inside walls.")]
    [SerializeField] private float itemRadius = 0.2f;

    private bool canInteract = true;
    private InteractableItem lookingAtItem;
    private PickableItem currentHeldItemR;
    private PickableItem currentHeldItemL;

    private InputAction interactAction;
    private InputAction dropAction;

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        dropAction = InputSystem.actions.FindAction("Throw");
    }

    public void SetInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public Transform GetPickupHandAndPickUpItem(PickableItem item)
    {
        if (currentHeldItemR == null)
        {
            currentHeldItemR = item;
            return holdPositionR;
        }
        if (currentHeldItemL == null)
        {
            currentHeldItemL = item;
            return holdPositionL;
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

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange))
        {
            InteractableItem interactableItem = hit.collider.GetComponentInParent<InteractableItem>();

            if (interactableItem != null && interactableItem.IsInteractable())
            {
                if (lookingAtItem != interactableItem)
                {
                    ClearHoverState();
                    lookingAtItem = interactableItem;
                    lookingAtItem.OnHoverEnter();
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

        if (lookingAtItem != null && interactAction.WasPressedThisFrame())
        {
            lookingAtItem.OnInteract(this);
        }

        if(currentHeldItemR != null) DoHeldItemStuff(currentHeldItemR, holdPositionR);
        else if(currentHeldItemL != null) DoHeldItemStuff(currentHeldItemL, holdPositionL);
    }

    private void ClearHoverState()
    {
        if (lookingAtItem != null)
        {
            lookingAtItem.OnHoverLeave();
            lookingAtItem = null;
        }
    }

    private void DoHeldItemStuff(PickableItem currentHeldItem, Transform holdPosition)
    {
        if (dropAction.WasPressedThisFrame())
        {
            DropItem(currentHeldItem, holdPosition);
        }
        else if(interactAction.WasPressedThisFrame() && lookingAtItem == null)
        {
            currentHeldItem.Use();
        }
    }

    private void DropItem(PickableItem currentHeldItem, Transform holdPosition)
    {
        currentHeldItem.transform.position = GetSafeDropPosition(holdPosition);

        if (player.IsSprinting())
        {
            Vector3 force = cameraTransform.forward * throwForceMultiplier;
            force += Vector3.up * (throwForceMultiplier * 0.2f);

            currentHeldItem.Throw(force);
        }
        else
        {
            currentHeldItem.Drop();
        }

        ClearItem(currentHeldItem);
    }

    private void ClearItem(PickableItem currentHeldItem)
    {
        if(currentHeldItem == currentHeldItemR) currentHeldItemR = null;
        else if(currentHeldItem == currentHeldItemL) currentHeldItemL = null;
    }

    private Vector3 GetSafeDropPosition(Transform holdPosition)
    {
        Vector3 origin = cameraTransform.position;
        Vector3 target = holdPosition.position;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (Physics.SphereCast(origin, itemRadius, direction.normalized, out RaycastHit hit, distance))
        {
            // If we hit a wall, place the item at the hit point, pushed slightly back along the surface normal
            return hit.point + (hit.normal * itemRadius);
        }

        // The path is clear, it's safe to drop it directly at the hand position
        return target;
    }
}