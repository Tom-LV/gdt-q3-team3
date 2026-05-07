using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterPusherBehavior : MonoBehaviour
{
    public float flowForce = 10f;
    public ParticleSystem waterParticles;

    private bool isActive = false;

    public bool isActiveAtStart = false;

    private void Start()
    {
        SetActive(isActiveAtStart);
    }

    // The TimelineEvent will call this
    public void SetActive(bool state)
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