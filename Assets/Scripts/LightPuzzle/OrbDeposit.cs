using UnityEngine;
using UnityEngine.Events;

public class OrbDeposit : MonoBehaviour
{
    [SerializeField] private Transform orb;
    [SerializeField] private UnityEvent start;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float maxTriggerDistance;
    private bool hasStarted = false;
    private Vector3 target;

    void Start()
    {
        target = transform.position + offset;
    }
    void Update()
    {
        if(!hasStarted && Vector3.Distance(orb.position, target) < maxTriggerDistance)
        {
            start?.Invoke();
        }
    }
}
