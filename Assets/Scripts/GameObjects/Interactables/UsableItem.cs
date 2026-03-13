using UnityEngine;

public abstract class UsableItem : Item
{
    [SerializeField] private float cooldown;
    private float cooldownTimer = 0;
    void Update()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    public bool Use(PlayerControls player)
    {
        if(cooldownTimer <= 0)
        {
            cooldownTimer = cooldown;
            UseItem(player);
        }
        return true;
    }
    protected abstract void UseItem(PlayerControls player);
}