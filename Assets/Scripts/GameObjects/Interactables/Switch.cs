using UnityEngine;

public class Switch : InteractableObject
{
    protected override bool CanInteract(PlayerControls player) { return true; }
    protected override void InteractObj(PlayerControls player)
    {
        EventTriggers.ActivatePuzzleSymbol("1");
    }
}
