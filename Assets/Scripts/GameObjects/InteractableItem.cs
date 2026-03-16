using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : MonoBehaviour
{
    private bool interactable = true;

    private int interactOutlineLayer;

    [SerializeField]
    UnityEvent m_OnInteract;

    public void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
    }

    public bool IsInteractable()
    {
        return interactable;
    }

    public void OnHoverEnter()
    {
        // Apply the outline layer to this object and all its children
        SetLayerRecursively(gameObject, interactOutlineLayer);
    }

    public void OnHoverLeave()
    {
        // Reset this object and all children back to layer 0 (Default)
        SetLayerRecursively(gameObject, 0);
    }

    public void OnInteract()
    {
        if (m_OnInteract != null)
        {
            m_OnInteract.Invoke();
        }
    }

    void Start()
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