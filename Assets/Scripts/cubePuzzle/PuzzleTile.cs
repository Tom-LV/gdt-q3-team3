using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleTile : MonoBehaviour
{
    [Tooltip("The element required to land on this tile. Set to 'None' for regular floor.")]
    public Element RequiredElement;

    private MeshRenderer TileRenderer;

    public bool IsLit { get; private set; } = false;

    private void Start()
    {
        TileRenderer = GetComponent<MeshRenderer>();
        SetLitState(false);
        var elements = Enum.GetValues(typeof(Element));

        foreach (Element element in elements)
        {
            var keyword = "_ELEMENT_" + element.ToString().ToUpper();
            if (element == RequiredElement)
                TileRenderer.material.EnableKeyword(keyword);
            else
                TileRenderer.material.DisableKeyword(keyword);
        }
    }

    public void SetLitState(bool lightUp)
    {
        IsLit = lightUp;

        TileRenderer.material.SetFloat("_Lit", lightUp ? 1f : 0f);
    }
}