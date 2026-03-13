using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : MonoBehaviour
{
    private bool interactable = true;
    private LayerMask interactOutlineLayer;
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
        gameObject.layer = interactOutlineLayer;
    }
    public void OnHoverLeave()
    {
        gameObject.layer = 0;
    }

    public void OnInteract()
    {
        if (m_OnInteract != null)
        {
            m_OnInteract.Invoke();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactOutlineLayer = LayerMask.NameToLayer("InteractOutline");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
