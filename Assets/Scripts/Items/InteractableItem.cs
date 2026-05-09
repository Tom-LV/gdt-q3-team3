using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : MonoBehaviour
{
    private bool interactable = true;
    private int interactOutlineLayer;

    [Header("Basic Interaction")]
    [SerializeField] protected UnityEvent m_OnInteract;
    [SerializeField] private string displayText;
    [SerializeField] protected float cooldown = 0.2f;

    [Header("Hold Settings")]
    [Tooltip("Check this to allow holding the interact button to trigger repeatedly.")]
    [SerializeField] private bool supportsHolding = false;
    [Tooltip("Delay before the continuous firing starts (e.g., wait 0.5s before rapid fire).")]
    [SerializeField] private float initialHoldDelay = 0.5f;
    [Tooltip("Time between each continuous fire (e.g., fire every 0.1s).")]
    [SerializeField] private float holdRepeatRate = 0.1f;

    protected float lastUseTime = float.NegativeInfinity;

    // Hold state tracking
    private float holdStartTime = float.NegativeInfinity;
    private bool isBeingHeld = false;

    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        // Failsafe: If interaction is disabled via script while holding, stop the hold
        if (!interactable) StopInteract();
    }

    public bool IsInteractable()
    {
        return interactable;
    }

    public virtual bool OffCooldown()
    {
        return Time.time - lastUseTime > cooldown;
    }

    public void OnHoverEnter()
    {
        SetLayerRecursively(gameObject, interactOutlineLayer);
    }

    public void OnHoverLeave()
    {
        SetLayerRecursively(gameObject, 0);
    }

    // Called ONCE when the button is initially pressed
    public virtual void OnInteract(PlayerInteract player)
    {
        if (!IsInteractable() || !OffCooldown()) return;

        TriggerInteraction();

        // If this item supports holding, start tracking the hold state
        if (supportsHolding)
        {
            isBeingHeld = true;
            holdStartTime = Time.time;
        }
    }

    // Called when the button is released, or the player looks away
    public virtual void StopInteract()
    {
        isBeingHeld = false;
    }

    protected virtual void Update()
    {
        // If the item is being held and is still interactable
        if (isBeingHeld && IsInteractable())
        {
            // 1. Check if the initial delay has passed
            if (Time.time - holdStartTime >= initialHoldDelay)
            {
                // 2. Check if enough time has passed since the LAST rapid fire
                if (Time.time - lastUseTime >= holdRepeatRate)
                {
                    TriggerInteraction();
                }
            }
        }
    }

    // Extracted the actual firing logic so both single-clicks and holds can use it securely
    protected virtual void TriggerInteraction()
    {
        if (m_OnInteract != null) m_OnInteract.Invoke();
        lastUseTime = Time.time;
    }

    protected virtual void Start()
    {
        interactOutlineLayer = LayerMask.NameToLayer("InteractOutline");
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = newLayer;
        }
    }
}