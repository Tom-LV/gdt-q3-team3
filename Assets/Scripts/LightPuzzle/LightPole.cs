using UnityEngine;
using System.Collections;

public class LightPole : PushableItem
{
    [SerializeField] private Collider storedItem;
    [SerializeField] private Transform topTransform;
    [SerializeField] protected Transform bottomTransform;
    [SerializeField] protected float collapseTime = 2f;
    [SerializeField] private float turnRadius;
    private float rotationOffset;

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
        float angle = Vector3.SignedAngle(relativeStartPos, dir, Vector3.up);
        pushObject.rotation = Quaternion.Euler(0f, angle + rotationOffset, 0f);
    }
    public override float[] GetAngleChange(Vector3 startPos, Vector3 endPos)
    {
        startPos -= pushObject.position;
        startPos.y = 0f;
        startPos.Normalize();

        endPos -= pushObject.position;
        endPos.y = 0f;
        endPos.Normalize();

        float[] angle = {0f, Vector3.SignedAngle(startPos, endPos, Vector3.up)};
        return angle;
    }
    public override void ExitInteraction() => rotationOffset = pushObject.eulerAngles.y;
    public virtual void Collapse()
    {
        Debug.Log("Tried Collapsing, " + IsInteractable());
        if (!IsInteractable()) return;
        SetInteractable(false);
        Debug.Log("Started Collapsing");
        StartCoroutine(CollapseRoutine()); // this is magic to avoid lag
    }
    private IEnumerator CollapseRoutine() // afforementioned magic
    {
        float t = 0f;
        Vector3 startPos = topTransform.position;
        Vector3 endPos = bottomTransform.position;

        while (t < collapseTime)
        {
            t += Time.deltaTime;
            float lerpT = t / collapseTime;
            topTransform.position = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
        Debug.Log("Finished Collapsing");
        FireBeam();
    }
    private void FireBeam()
    {
        Vector3 origin = topTransform.position;
        Vector3 direction = topTransform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            DrawBeam(origin, hit.point);

            LightPole other = hit.collider.GetComponentInParent<LightPole>();
            other?.Collapse();
        }
        else
        {
            DrawBeam(origin, origin + direction * 100f);
        }
    }

    // light stuff
    private LineRenderer lr;
    void Awake()
    {
        if(storedItem != null) Physics.IgnoreCollision(topTransform.GetComponent<Collider>(), storedItem);

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Unlit/Color")) {color = Color.yellow};

        rotationOffset = pushObject.eulerAngles.y;
        Debug.Log("offset: " + rotationOffset);
    }
    public void DrawBeam(Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
