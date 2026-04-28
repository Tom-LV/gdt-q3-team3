using System.Collections;
using UnityEngine;

public class NotificationGlow : MonoBehaviour
{
    [Header("Setup")]
    public MeshRenderer GlowRenderer;

    [Header("Glow Settings")]
    // The second 'true' tells Unity to show the HDR color picker in the Inspector!
    [ColorUsage(true, true)] public Color BaseGlowColor = Color.cyan;

    public float MaxIntensity = 4.0f; // Push this higher for more bloom!
    public float FadeInSpeed = 10f;
    public float FadeOutSpeed = 3f;
    public int PulseCount = 2;

    private Material _mat;
    private Coroutine _glowRoutine;
    private int _colorPropertyID;

    private void Awake()
    {
        if (GlowRenderer == null) GlowRenderer = GetComponent<MeshRenderer>();

        // This creates an instance of the material so we don't accidentally edit the project file
        _mat = GlowRenderer.material;

        // URP uses _BaseColor, Built-in uses _Color. This checks which one your shader uses!
        _colorPropertyID = _mat.HasProperty("_BaseColor") ? Shader.PropertyToID("_BaseColor") : Shader.PropertyToID("_Color");

        // Start fully invisible
        _mat.SetColor(_colorPropertyID, Color.black);
    }

    // Call this from your StoryRunner or PhoneOS when a message arrives!
    public void TriggerGlow()
    {
        if (PhoneController.isGamePaused) return;
        if (_glowRoutine != null) StopCoroutine(_glowRoutine);
        _glowRoutine = StartCoroutine(GlowSequence());
    }

    private IEnumerator GlowSequence()
    {
        for (int i = 0; i < PulseCount; i++)
        {
            float t = 0;

            // 1. Flash Up
            while (t < 1f)
            {
                t += Time.deltaTime * FadeInSpeed;
                float currentIntensity = Mathf.Lerp(0, MaxIntensity, t);
                _mat.SetColor(_colorPropertyID, BaseGlowColor * currentIntensity);
                yield return null;
            }

            t = 0;

            // 2. Fade Down
            while (t < 1f)
            {
                t += Time.deltaTime * FadeOutSpeed;
                float currentIntensity = Mathf.Lerp(MaxIntensity, 0, t);
                _mat.SetColor(_colorPropertyID, BaseGlowColor * currentIntensity);
                yield return null;
            }

            // Optional: Add a tiny micro-pause between pulses so they feel distinct
            yield return new WaitForSeconds(0.05f);
        }

        // 3. Ensure it is completely turned off at the very end
        _mat.SetColor(_colorPropertyID, Color.black);
    }
}