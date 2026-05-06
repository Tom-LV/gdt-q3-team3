using UnityEngine;

public class WaterCurrent : MonoBehaviour
{
    [Header("Dials")]
    public TimerDial dial;

    [Header("Flow Settings")]
    public Vector3 flowDirection = Vector3.forward;
    public float flowForce = 10f;
    public ParticleSystem waterParticles; // Optional: to visually show flow turning on/off

    private bool isActive = false;

    private void Start()
    {
        if (waterParticles != null)
        {
            if (isActive) waterParticles.Play();
            else waterParticles.Stop();
        }

    }

    // Called by the Manager every frame the puzzle is playing
    public void EvaluateTime(float elapsedTime)
    {
        // If the elapsed time is between the start and stop dial settings, turn on
        if (elapsedTime >= dial.startTime && elapsedTime <= dial.startTime + dial.length)
        {
            if (!isActive) SetFlowActive(true);
        }
        else
        {
            if (isActive) SetFlowActive(false);
        }
    }

    public void SetFlowActive(bool state)
    {
        isActive = state;
        if (waterParticles != null)
        {
            if (state) waterParticles.Play();
            else waterParticles.Stop();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("PuzzleBall"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(transform.forward * flowForce, ForceMode.Acceleration);
            }
        }
    }
}
