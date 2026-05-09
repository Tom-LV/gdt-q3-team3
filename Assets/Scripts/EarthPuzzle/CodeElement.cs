using UnityEngine;

public class CodeElement : MonoBehaviour
{
    [Tooltip("number between 0-4 that is the correct state, when rotating the bolts upwards")]
    [SerializeField] private int correctState;
    private readonly Quaternion rotationAmount = Quaternion.Euler(0f, 360f / 5, 0f);
    private int state = 0;
    private Quaternion targetRotation;
    private Quaternion startRotation;
    private float startTime;
    void Start()
    {
        targetRotation = transform.rotation;
        startRotation = targetRotation;
    }
    public void Rotate()
    {
        targetRotation *= rotationAmount;
        startRotation = transform.rotation;
        state = (state + 1) % 5; 
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
    public bool IsInCorrectState()
    {
        return state == correctState;
    }
}
