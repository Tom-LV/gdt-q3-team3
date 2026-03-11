using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlateManager : MonoBehaviour
{
    [Header("Master Glyph Library")]
    [Tooltip("Drag your prefabs here. Item 0 = Key 1, Item 1 = Key 2, Item 2 = Key 3, Item 3 = Key 4")]
    public List<GameObject> masterGlyphs;

    private PlateScript[] allPlates;

    void Start()
    {
        // Find all the slates that are children of this manager
        allPlates = GetComponentsInChildren<PlateScript>();

        // Automatically assign IDs to all plates based on their prefabs
        foreach (PlateScript plate in allPlates)
        {
            plate.myGlyphIDs.Clear();

            foreach (GameObject prefab in plate.glyphPrefabs)
            {
                // Find where this prefab sits in the master list
                int index = masterGlyphs.IndexOf(prefab);

                if (index != -1)
                {
                    plate.myGlyphIDs.Add(index + 1);
                }
                else
                {
                    Debug.LogWarning($"Prefab {prefab.name} on {plate.gameObject.name} is not in the Master List!");
                }
            }
        }
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) OnGlyphPressed(1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) OnGlyphPressed(2);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) OnGlyphPressed(3);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) OnGlyphPressed(4);
    }

    private void OnGlyphPressed(int pressedGlyphID)
    {
        foreach (PlateScript plate in allPlates)
        {
            if (plate.myGlyphIDs.Contains(pressedGlyphID))
            {
                plate.ToggleActive();
            }
        }
    }
}
