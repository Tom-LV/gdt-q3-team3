using UnityEngine;
using System.Collections;

public class LightToggle : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Light component you want to toggle.")]
    [SerializeField] private Light targetLight;

    [Tooltip("The MeshRenderer on the child object holding the materials.")]
    [SerializeField] private MeshRenderer targetRenderer;

    [Header("Material Settings")]
    [Tooltip("Which material index has the bloom/emission?")]
    [SerializeField] private int emissiveMaterialIndex = 1;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.5f;
    [Tooltip("Does the light start turned on?")]
    [SerializeField] private bool isOn = true;

    // Internal state tracking
    private Material emissiveMaterial;
    private float maxIntensity;
    private Color maxEmissionColor;
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        // Cache the initial "On" values so we know what to fade back up to
        if (targetLight != null)
        {
            maxIntensity = targetLight.intensity;
        }

        if (targetRenderer != null && targetRenderer.materials.Length > emissiveMaterialIndex)
        {
            // Using .materials creates a unique instance of the material for this specific object.
            // This ensures turning off one light doesn't turn off every light in your game!
            emissiveMaterial = targetRenderer.materials[emissiveMaterialIndex];
            emissiveMaterial.EnableKeyword("_EMISSION"); // Ensure emission is active

            // Built-in & URP use "_EmissionColor". HDRP uses "_EmissiveColor" LDR/HDR.
            maxEmissionColor = emissiveMaterial.GetColor("_EmissionColor");
        }
        else
        {
            Debug.LogWarning("SmoothLightToggle: Renderer is missing or material index is out of bounds!", this);
        }

        // Set the initial state instantly without animation
        SnapToState(isOn);
    }

    /// <summary>
    /// Call this method from a button, trigger, or interaction script to flip the light state.
    /// </summary>
    public void Toggle()
    {
        SetState(!isOn);
    }

    /// <summary>
    /// Explicitly turn the light on (true) or off (false) with animation.
    /// </summary>
    public void SetState(bool turnOn)
    {
        if (isOn == turnOn) return; // Already in that state

        isOn = turnOn;

        // Stop the current animation if it's currently halfway through fading
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(AnimateLight(isOn));
    }

    private IEnumerator AnimateLight(bool turningOn)
    {
        float elapsedTime = 0f;

        // Grab the exact values we are starting from right now (useful if interrupted mid-fade)
        float startIntensity = targetLight != null ? targetLight.intensity : 0f;
        Color startEmission = emissiveMaterial != null ? emissiveMaterial.GetColor("_EmissionColor") : Color.black;

        // Determine our target values
        float targetIntensity = turningOn ? maxIntensity : 0f;
        Color targetEmission = turningOn ? maxEmissionColor : Color.black;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // SmoothStep makes the fade ease-in and ease-out nicely, rather than being linear and stiff
            float smoothedT = Mathf.SmoothStep(0f, 1f, t);

            if (targetLight != null)
            {
                targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothedT);
            }

            if (emissiveMaterial != null)
            {
                emissiveMaterial.SetColor("_EmissionColor", Color.Lerp(startEmission, targetEmission, smoothedT));
            }

            yield return null;
        }

        // Ensure we end up exactly on the target values
        SnapToState(turningOn);
    }

    // Instantly sets the values (used on Awake and at the very end of the Coroutine)
    private void SnapToState(bool state)
    {
        if (targetLight != null)
        {
            targetLight.intensity = state ? maxIntensity : 0f;
            targetLight.enabled = state || maxIntensity > 0f; // Optional: completely disable component if off
        }

        if (emissiveMaterial != null)
        {
            emissiveMaterial.SetColor("_EmissionColor", state ? maxEmissionColor : Color.black);
        }
    }
}