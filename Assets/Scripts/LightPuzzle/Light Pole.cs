using UnityEngine;

public class LightPole : PushableItem
{
    [SerializeField] private Transform topTransform;
    [SerializeField] private float turnRadius;

    public override void OnInteract(PlayerInteract player)
    {
        if (!OffCooldown()) return;

        player.StartPushInteraction(this);
    }
    public override Vector3 FindNearestPointOnPath(Vector3 pos)
    {
        Vector3 dir = pos - pushObject.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return pushObject.position + Vector3.forward * turnRadius;
        dir.Normalize();
        return pushObject.position + dir * turnRadius;
    }
    public override Quaternion FindOrientationOfPointOnPath(Vector3 pos)
    {
        Vector3 radiusDir = pos - pushObject.position;
        radiusDir.y = 0f;
        radiusDir.Normalize();
        Vector3 tangent = Vector3.Cross(Vector3.up, radiusDir);
        return Quaternion.LookRotation(tangent);
    }
    public override void PushToPlayerPos(Vector3 pos)
    {
        Vector3 dir = pos - pushObject.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        topTransform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
    public void Collapse()
    {
        SetInteractable(false);
        // move the stuff together
    }
}
