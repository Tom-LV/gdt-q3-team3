using System;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]
    private float interactRange = 5f;
    [SerializeField]
    private Transform cameraTransform;
    private bool canInteract = true;

    private InteractableItem lookingAtItem;

    public void SetInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }

    public void SetInteractRange(float interactRange)
    {
        if (interactRange < 0)
        {
            Debug.LogWarning("Interact range cannot be negative");
        }
        this.interactRange = interactRange;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Check if we are allowed to interact
        if (!canInteract || PhoneController.isGamePaused)
        {
            // If the phone opens while we are hovering over an item, clear the hover state
            if (lookingAtItem != null)
            {
                lookingAtItem.OnHoverLeave();
                lookingAtItem = null;
            }
            return; // Stop running the rest of the interact logic
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactRange))
        {
            InteractableItem interactableItem = hit.collider.GetComponentInParent<InteractableItem>();

            if (interactableItem != null && !interactableItem.IsInteractable())
            {
                interactableItem = null;
            }

            if (interactableItem == null && lookingAtItem != null)
            {
                lookingAtItem.OnHoverLeave();
                lookingAtItem = null;
            }

            if (interactableItem != null)
            {
                if (lookingAtItem == null)
                {
                    lookingAtItem = interactableItem;
                    lookingAtItem.OnHoverEnter();
                }
                else if (lookingAtItem != interactableItem)
                {
                    lookingAtItem.OnHoverLeave();
                    lookingAtItem = interactableItem;
                    lookingAtItem.OnHoverEnter();
                }
            }
        }
        else if (lookingAtItem != null)
        {
            lookingAtItem.OnHoverLeave();
            lookingAtItem = null;
        }

        if (lookingAtItem != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            lookingAtItem.OnInteract();
        }
    }
}
