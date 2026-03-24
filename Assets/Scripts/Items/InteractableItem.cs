using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : MonoBehaviour
{
    private bool interactable = true;
    private int interactOutlineLayer;

    [SerializeField] protected UnityEvent m_OnInteract;
    [SerializeField] private string displayText;
    [SerializeField] protected float cooldown;

    protected float lastUseTime = float.NegativeInfinity;
    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
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
    public virtual void OnInteract(PlayerInteract player)
    {
        if(!IsInteractable() || !OffCooldown()) return;
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