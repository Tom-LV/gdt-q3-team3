using UnityEngine;

public class Lock : InteractableItem
{
    public int keyID;
    public override void OnInteract(PlayerInteract playerInteract)
    {
        if(!IsInteractable() || !OffCooldown()) return;
        PickableItem key = playerInteract.GetKeyItem(keyID);
        if(key == null) return;
        if (m_OnInteract != null) m_OnInteract.Invoke();
        lastUseTime = Time.time;
    }
}
