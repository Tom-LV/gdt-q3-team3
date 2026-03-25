using UnityEngine;

public abstract class PushableItem : InteractableItem
{
    [SerializeField] protected Transform pushObject;
    [SerializeField] protected Vector3 relativeStartPos;
    private Transform tr;


    protected override void Start()
    {
        base.Start();
        cooldown = 1f;
        tr = GetComponent<Transform>();
    }
    public override void OnInteract(PlayerInteract player)
    {
        if (!OffCooldown()) return;
        player.StartPushInteraction(this);
        base.OnInteract(player);
    }
    public Vector3 FindStartPointOnPath() => pushObject.position + relativeStartPos;
    public abstract Vector3 FindNearestPointOnPath(Vector3 pos);
    public abstract Quaternion FindStartOrientation();
    public abstract void PushToPlayerPos(Vector3 pos);
    public virtual void ExitInteraction()
    {
        return;
    }
    public virtual float[] GetAngleChange(Vector3 startPos, Vector3 endPos) => new float[2];
}