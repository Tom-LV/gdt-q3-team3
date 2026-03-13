using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string interactName;
    public abstract void Interact();
}
