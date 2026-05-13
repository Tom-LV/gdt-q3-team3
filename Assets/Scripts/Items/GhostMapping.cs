using UnityEngine;

[System.Serializable]
public class GhostMapping
{
    [Tooltip("Item ID.")]
    public string type;
    [Tooltip("The transparent ghost model for this specific item.")]
    public GameObject ghostVisual;
}