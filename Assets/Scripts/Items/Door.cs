using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [SerializeField] protected UnityEvent m_OnOpen;
    [SerializeField] private bool startOpen = false;
    [SerializeField] private Transform door;
    [SerializeField] private Vector3 relativeOpenPos;
    [SerializeField] private float openTime = 4;
    private Vector3 closedPos;
    void Awake()
    {
        closedPos = door.localPosition;
        if(startOpen) Open();
    }
    public void Open()
    {
        StartCoroutine(OpenRoutine());
    }
    
    private IEnumerator OpenRoutine()
    {
        float t = 0f;
        Vector3 startPos = door.localPosition;
        Vector3 endPos = startPos + relativeOpenPos;

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
        StartCoroutine(CloseRoutine());
    }
    private IEnumerator CloseRoutine()
    {
        float t = 0f;
        Vector3 startPos = door.localPosition;
        Vector3 endPos = closedPos;

        while (t < openTime)
        {
            t += Time.deltaTime;
            float lerpT = t / openTime;
            door.localPosition = Vector3.Lerp(startPos, endPos, lerpT);

            yield return null;
        }
        if (m_OnOpen!= null) m_OnOpen.Invoke();
    }
}
