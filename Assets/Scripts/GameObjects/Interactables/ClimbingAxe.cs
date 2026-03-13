using UnityEngine;

public class ClimbingAxe : UsableItem
{
    [SerializeField] private float pullForce;
    protected override void UseItem(PlayerControls player)
    {
        player.SetVelocity(Vector3.zero);
        player.AddForceImpulse(player.GetPivotForward() * pullForce);
    }
}
