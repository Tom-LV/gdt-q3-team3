using UnityEngine;
using UnityEngine.Events;

public class OrbDeposit : MonoBehaviour
{
    [SerializeField] private UnityEvent start;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float maxTriggerDistance;
    [SerializeField] private Transform resetBowl;
    private bool hasStarted = false;
    private Vector3 target;

    void Start()
    {
        target = transform.position + offset;
        PickableItem item = GetOrbNearby();
        if(item == null) return;
        item.transform.position = resetBowl.position + offset;
        item.SetInteractable(true);
    }
    void Update()
    {
        PickableItem item = GetOrbNearby();
        if(!hasStarted && item != null)
        {
            hasStarted = true;
            item.SetInteractable(false);
            start?.Invoke();
        }
    }

    private PickableItem GetOrbNearby()
    {
        Collider[] colliders = Physics.OverlapSphere(target, maxTriggerDistance);
        if(colliders.Length > 3) return null;
        foreach (Collider col in colliders)
        {
            PickableItem item = col.GetComponent<PickableItem>();
            if(item != null && item.HasType("50"))
            {
                return item;
            }
        }
        return null;
    }
}
