using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string interactName;
    [SerializeField] private float cooldown;
    private float cooldownTimer = 0;
    void Update()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    public bool Interact(PlayerControls player)
    {
        if(cooldownTimer <= 0 && CanInteract(player))
        {
            cooldownTimer = cooldown;
            InteractObj(player);
        }
        return false;
    }
    protected abstract bool CanInteract(PlayerControls player);
    protected abstract void InteractObj(PlayerControls player);
}
