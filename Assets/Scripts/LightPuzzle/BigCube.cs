using UnityEngine;

public class BigCube : PushableItem
{
    private Rigidbody rb;
    protected override void Start()
    {
        base.Start();
        rb = pushObject.GetComponent<Rigidbody>();
    }
    public override Vector3 FindNearestPointOnPath(Vector3 pos)
    {
        Vector3 zVector = new Vector3(0, 0, 1);
        Vector3 diff = pos - tr.position;
        Vector3 relativeReturnPos = Vector3.Project(diff, zVector);
        return tr.position + Vector3.ClampMagnitude(relativeReturnPos, 3);
    }
    public override Quaternion FindStartOrientation()
    {
        return new Quaternion(0, 1, 0, 0);
    }
    public override void PushToPlayerPos(Vector3 pos)
    {
        Vector3 currentPos = rb.position;
        Vector3 targetPos = currentPos;
        targetPos.z = pos.z - 2;

        Vector3 direction = targetPos - currentPos;
        float distance = direction.magnitude;

        if (!rb.SweepTest(direction.normalized, out RaycastHit hit, distance))
        {
            rb.MovePosition(targetPos);
        }
    }
}
