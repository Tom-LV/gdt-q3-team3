using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private Transform cameraTransform;

    [Header("Hold Settings")]
    public Transform holdPosition;
    [SerializeField] private float throwForceMultiplier = 12f;

    [Tooltip("How thick the item is. Used to prevent dropping items inside walls.")]
    [SerializeField] private float itemRadius = 0.2f;

    private bool canInteract = true;
    private InteractableItem lookingAtItem;
    private PickableItem currentHeldItem;

    private InputAction interactAction;
    private InputAction throwAction;

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
        throwAction = InputSystem.actions.FindAction("Throw");
    }

    public void SetInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public void SetHeldItem(PickableItem item)
    {
        currentHeldItem = item;
    }

    void Update()
    {
        if (!canInteract || PhoneController.isGamePaused)
        {
            ClearHoverState();
            return;
        }

        bool interactPressed = interactAction.WasPressedThisFrame();
        bool throwPressed = throwAction != null && throwAction.WasPressedThisFrame();

        // Holding item
        if (currentHeldItem != null)
        {
            ClearHoverState();

            if (throwPressed)
            {
                // 1. Move the item to a safe position so it doesn't clip through walls
                currentHeldItem.transform.position = GetSafeDropPosition();

                Vector3 force = cameraTransform.forward * throwForceMultiplier;
                force += Vector3.up * (throwForceMultiplier * 0.2f);

                currentHeldItem.Throw(force);
                currentHeldItem = null;
            }
            else if (interactPressed)
            {
                // 1. Move the item to a safe position
                currentHeldItem.transform.position = GetSafeDropPosition();

                currentHeldItem.Drop();
                currentHeldItem = null;
            }

            return;
        }

        // Not holding item
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

        if (lookingAtItem != null && interactPressed)
        {
            lookingAtItem.OnInteract(this);
        }
    }

    private void ClearHoverState()
    {
        if (lookingAtItem != null)
        {
            lookingAtItem.OnHoverLeave();
            lookingAtItem = null;
        }
    }

    private Vector3 GetSafeDropPosition()
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