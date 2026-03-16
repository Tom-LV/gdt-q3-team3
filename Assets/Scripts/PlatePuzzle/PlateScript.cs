using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlateScript : MonoBehaviour
{
    [Header("Glyphs to Place (1 to 4)")]
    [Tooltip("Drag the prefabs of the hieroglyphs you want on this plate.")]
    public List<GameObject> glyphPrefabs;

    [Header("Placement Settings")]
    [Tooltip("Create an Empty GameObject right on the top center of the plate and assign it here.")]
    public Transform surfaceCenter;
    public float spacing = 0.5f;
    public float heightOffset = 0.02f; // Keeps the glyphs from clipping into the stone
    public Boolean isActive = false;
    public float powerOnValue = 2f;
    public float powerOffValue = 0.1f;

    public UnityEvent<Boolean> m_OnToggle;

    [HideInInspector]
    public List<int> myGlyphIDs = new List<int>();
    private List<Renderer> spawnedRenderers = new List<Renderer>();

    void Start()
    {
        ArrangeGlyphs();
        SetState(isActive);
    }

    void ArrangeGlyphs()
    {
        int count = glyphPrefabs.Count;

        if (count == 0) return;

        // Cap the maximum at 4
        if (count > 4) count = 4;

        for (int i = 0; i < count; i++)
        {
            if (glyphPrefabs[i] == null) continue;

            // Instantiate the glyph and make it a child of the surface center
            GameObject glyph = Instantiate(glyphPrefabs[i], surfaceCenter);

            Renderer glyphRenderer = glyph.GetComponentInChildren<Renderer>();
            if (glyphRenderer != null)
            {
                spawnedRenderers.Add(glyphRenderer);
            }

            Vector3 localPos = Vector3.zero;
            float scale = 75f;

            // Determine layout based on how many glyphs there are
            switch (count)
            {
                case 1:
                    localPos = new Vector3(0, heightOffset, 0);
                    scale = 60f;
                    break;

                case 2:
                    float xOffset2 = (i == 0) ? -spacing : spacing;
                    localPos = new Vector3(xOffset2, heightOffset, 0);
                    scale = 60f;
                    break;

                case 3:
                    float xOffset3 = (i - 1) * spacing;
                    float yOffset3 = (i == 1) ? 0.2f : -0.2f;
                    localPos = new Vector3(xOffset3, heightOffset, yOffset3);
                    scale = 60f;
                    break;

                case 4:
                    float xOffset4 = (i % 2 == 0) ? -spacing : spacing;
                    float zOffset4 = (i < 2) ? spacing : -spacing;
                    localPos = new Vector3(xOffset4, heightOffset, zOffset4);
                    scale = 60f;
                    break;
            }

            // Apply the calculated position and scale
            glyph.transform.localPosition = localPos;
            glyph.transform.localScale = Vector3.one * scale;
            glyph.transform.localScale = new Vector3(glyph.transform.localScale.x, glyph.transform.localScale.y, glyph.transform.localScale.z * 10);
        }
    }

    public void ToggleActive()
    {
        isActive = !isActive;
        float targetPower = isActive ? powerOnValue : powerOffValue;
        transform.localPosition = new Vector3(transform.localPosition.x, isActive ? 0.05f : 0f, transform.localPosition.z);
        SetGlyphPower(targetPower);
    }

    private void SetState(bool isActive)
    {
        this.isActive = isActive;
        float targetPower = isActive ? powerOnValue : powerOffValue;
        transform.localPosition = new Vector3(transform.localPosition.x, isActive ? 0.05f : 0f, transform.localPosition.z);
        SetGlyphPower(targetPower);
    }

    private void SetGlyphPower(float powerValue)
    {
        if (m_OnToggle != null) m_OnToggle.Invoke(isActive);

        foreach (Renderer r in spawnedRenderers)
        {
            if (r != null)
            {
                r.material.SetFloat("_power", powerValue);
                
            }
        }
    }
}
