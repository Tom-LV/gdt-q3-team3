using UnityEngine;

public class CodeElement : MonoBehaviour
{
    private readonly Quaternion rotationAmount = Quaternion.Euler(0f, 360f / 5, 0f);
    private Quaternion targetRotation = Quaternion.identity;
    private Quaternion startRotation = Quaternion.identity;
    private float startTime;
    public void Rotate()
    {
        targetRotation *= rotationAmount;
        startRotation = transform.rotation;
        startTime = Time.time;
    }
    void Update()
    {
        transform.rotation = Quaternion.Lerp(startRotation, targetRotation, Mathf.Clamp((Time.time - startTime) * 2, 0, 1));
        if((Time.time - startTime) * 2 > 1)
        {
            transform.rotation = targetRotation;
        }
    }
}
