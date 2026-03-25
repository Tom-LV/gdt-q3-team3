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
    public override Quaternion FindStartOrientation()
    {
        Vector3 dir = relativeStartPos;
        dir.y = 0f;
        dir.Normalize();
        Vector3 tangent = Vector3.Cross(Vector3.up, dir);
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
    public override Quaternion GetAngleChange(Vector3 startPos, Vector3 endPos)
    {
        Vector3 dir = startPos;
        dir.y = 0f;
        dir.Normalize();
        Vector3 startAngle = Vector3.Cross(Vector3.up, dir);

        dir = endPos;
        dir.y = 0f;
        dir.Normalize();
        Vector3 endAngle = Vector3.Cross(Vector3.up, dir);

        return Quaternion.FromToRotation(startAngle, endAngle);
    }
    public void Collapse()
    {
        SetInteractable(false);
        // move the stuff together
    }
}
