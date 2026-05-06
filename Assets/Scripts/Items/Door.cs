using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [SerializeField] protected UnityEvent m_OnOpen;
    [SerializeField] private Transform door;
    [SerializeField] private float doorHeight;
    [SerializeField] private float openTime = 4;
    private bool isOpen;
    private Rigidbody rb;

    void Awake()
    {
        rb = door.GetComponent<Rigidbody>(); // fix rotation and xz movement (just y movement)
    }
    public void Open()
    {
        isOpen = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        StartCoroutine(OpenRoutine()); // second instance of the magic Coroutine
    }
    
    private IEnumerator OpenRoutine() // afforementioned magic
    {
        float t = 0f;
        Vector3 startPos = door.localPosition;
        Vector3 endPos = startPos + new Vector3(0f, doorHeight, 0f);

        while (t < openTime)
        {
            t += Time.deltaTime;
            float lerpT = t / openTime;
            door.localPosition = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
        if (m_OnOpen!= null) m_OnOpen.Invoke();
    }
    public void Close()
    {
        rb.linearVelocity = Vector3.zero;
        isOpen = false;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }
}
