using UnityEngine;

public class RotatorBehavior : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 90f;

    private bool isRotating = false;

    private void Start()
    {
        ResetRotator();
    }

    // The TimelineEvent will call this
    public void SetActive(bool state)
    {
        isRotating = state;
    }

    void Update()
    {
        if (isRotating)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
    }

    public void ResetRotator()
    {
        transform.rotation = Quaternion.identity;
    }
}