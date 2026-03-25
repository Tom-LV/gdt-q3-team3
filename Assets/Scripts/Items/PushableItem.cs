using UnityEngine;

public abstract class PushableItem : InteractableItem
{
    [SerializeField] protected Transform pushObject;
    [SerializeField] private Vector3 relativeStartPos;
    private Transform tr;


    protected override void Start()
    {
        base.Start();
        cooldown = 1f;
        tr = GetComponent<Transform>();
    }
    public Vector3 FindStartPointOnPath() => pushObject.position + relativeStartPos;
    public abstract Vector3 FindNearestPointOnPath(Vector3 pos);
    public abstract Quaternion FindOrientationOfPointOnPath(Vector3 pos);
    public abstract void PushToPlayerPos(Vector3 pos);
}