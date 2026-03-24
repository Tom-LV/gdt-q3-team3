using UnityEngine;

public class LavaPlatform : MonoBehaviour
{
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float lifeTime;

    [SerializeField]
    private float floorDetectionHeight;

    private float age = 0f;
    private BoxCollider platformCollider;

    void Start()
    {
        platformCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        age += Time.deltaTime;
        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(this.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
    }
}