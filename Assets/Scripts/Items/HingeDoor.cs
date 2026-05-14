using UnityEngine;

public class HingeDoor : MonoBehaviour
{
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 localClosePos;
    [SerializeField] private Vector3 localCloseRot;
    [SerializeField] private Vector3 localOpenPos;
    [SerializeField] private Vector3 localOpenRot;
    [SerializeField] private float openTime;
    private Quaternion openRot;
    private Quaternion closedRot;
    private Vector3 localPivot;
    private Vector3 axis;
    private float angleDeg;
    private float t = 0f;
    private bool open = false;
    void Start()
    {
        openRot = Quaternion.Euler(localOpenRot);
        closedRot = Quaternion.Euler(localCloseRot);
        Quaternion deltaRot = openRot * Quaternion.Inverse(closedRot);
        
        deltaRot.ToAngleAxis(out float angleDegOUT, out Vector3 axisOUT);
        angleDeg = angleDegOUT;
        axis = axisOUT;
        localPivot = CalculatePivot(localClosePos, localOpenPos, axis, angleDeg);
    }

    private Vector3 CalculatePivot(Vector3 p0, Vector3 p1, Vector3 axis, float angleDeg)
    {
        float theta = angleDeg * Mathf.Deg2Rad;

        Vector3 chord = p1 - p0;
        float d = chord.magnitude;
        float radius = d / (2f * Mathf.Sin(theta / 2f));
        Vector3 midpoint = (p0 + p1) * 0.5f;

        Vector3 chordDir = chord.normalized;
        Vector3 towardCenter = Vector3.Cross(axis, chordDir).normalized;

        float h = Mathf.Sqrt(radius * radius - (d * d * 0.25f));

        return midpoint + towardCenter * h; // midpoint - towardCenter * h is also valid
    }

    public void Open()
    {
        open = true;
    }

    public void Close()
    {
        open = false;
    }

    private void Update()
    {
        float delta = Time.deltaTime / openTime;
        t += open ? delta : -delta;
        t = Mathf.Clamp01(t);

        Quaternion targetRot = Quaternion.Slerp(closedRot, openRot, t);
        ApplyHingeRotation(targetRot);
    }

    private void ApplyHingeRotation(Quaternion localRot)
    {
        Quaternion delta = localRot * Quaternion.Inverse(closedRot);

        Vector3 closedOffset = localClosePos - localPivot;
        Vector3 rotatedOffset = delta * closedOffset;

        door.localPosition = localPivot + rotatedOffset;
        door.localRotation = localRot;
    }
}
