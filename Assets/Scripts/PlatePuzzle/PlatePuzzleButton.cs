using UnityEngine;

public class PlatePuzzleButton : MonoBehaviour
{
    float powerValue = 2.0f;
    bool isOn = false;
    public Renderer glyphRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        powerValue = isOn ? 2.0f : 0.1f;
        glyphRenderer.material.SetFloat("_power", powerValue);
    }

    public void OnToggle()
    {
        isOn = !isOn;
        powerValue = isOn ? 2.0f : 0.1f;
        glyphRenderer.material.SetFloat("_power", powerValue);
    }
}
