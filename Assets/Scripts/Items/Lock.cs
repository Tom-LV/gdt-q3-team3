using UnityEngine;

public class Lock : InteractableItem
{
    public string type;
    public override void OnInteract(PlayerInteract playerInteract)
    {
        if(!IsInteractable() || !OffCooldown()) return;
        PickableItem key = playerInteract.GetKeyItem(type);
        if(key == null) return;
        if (m_OnInteract != null) m_OnInteract.Invoke();
        lastUseTime = Time.time;
    }
}
